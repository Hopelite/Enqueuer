using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Enqueuer.Callbacks.CallbackHandlers.BaseClasses;
using Enqueuer.Callbacks.Extensions;
using Enqueuer.Data;
using Enqueuer.Data.Constants;
using Enqueuer.Data.DataSerialization;
using Enqueuer.Data.TextProviders;
using Enqueuer.Persistence.Extensions;
using Enqueuer.Services;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Queue = Enqueuer.Persistence.Models.Queue;
using User = Enqueuer.Persistence.Models.User;

namespace Enqueuer.Callbacks.CallbackHandlers;

public class GetQueueCallbackHandler : CallbackHandlerBaseWithRemoveQueueButton
{
    private readonly IQueueService _queueService;
    private readonly IUserService _userService;
    private readonly ILogger<EnqueueMeCallbackHandler> _logger;

    public GetQueueCallbackHandler(ITelegramBotClient telegramBotClient, IQueueService queueService, IUserService userService, IDataSerializer dataSerializer, IMessageProvider messageProvider, ILogger<EnqueueMeCallbackHandler> logger)
        : base(telegramBotClient, dataSerializer, messageProvider)
    {
        _queueService = queueService;
        _userService = userService;
        _logger = logger;
    }

    protected override Task HandleAsyncImplementation(Callback callback)
    {
        if (callback.CallbackData?.QueueData == null)
        {
            _logger.LogWarning("Handled the outdated callback.");
            return TelegramBotClient.EditMessageTextAsync(
                callback.Message.Chat,
                callback.Message.MessageId,
                MessageProvider.GetMessage(CallbackMessageKeys.OutdatedCallback_Message),
                ParseMode.Html);
        }

        return HandleAsyncInternal(callback);
    }

    private async Task HandleAsyncInternal(Callback callback)
    {
        var queue = await _queueService.GetQueueAsync(callback.CallbackData!.QueueData.QueueId, includeMembers: true, CancellationToken.None);
        if (queue == null)
        {
            var returnButton = GetReturnToChatButton(callback.CallbackData);
            await TelegramBotClient.EditMessageTextAsync(
                callback.Message.Chat,
                callback.Message.MessageId,
                MessageProvider.GetMessage(CallbackMessageKeys.QueueHasBeenDeleted_Message),
                replyMarkup: returnButton);

            return;
        }

        await HandleCallbackWithExistingQueueAsync(callback, queue);
    }

    private async Task HandleCallbackWithExistingQueueAsync(Callback callback, Queue queue)
    {
        var user = await _userService.GetOrStoreUserAsync(callback.From, CancellationToken.None);

        var replyMarkup = await BuildReplyMarkup(user, queue, callback.CallbackData!);
        var responseMessage = BuildResponseMessage(queue);

        await TelegramBotClient.EditMessageTextAsync(
            callback.Message.Chat,
            callback.Message.MessageId,
            responseMessage,
            ParseMode.Html,
            replyMarkup: replyMarkup);
    }

    private async Task<InlineKeyboardMarkup> BuildReplyMarkup(User user, Queue queue, CallbackData callbackData)
    {
        var doesUserParticipate = user.IsParticipatingIn(queue);
        var replyMarkupButtons = new List<InlineKeyboardButton[]>()
        {
            doesUserParticipate
            ? new InlineKeyboardButton[] { GetQueueRelatedButton(MessageProvider.GetMessage(CallbackMessageKeys.GetQueueCallbackHandler.GetQueueCallback_DequeueMe_Button), CallbackConstants.DequeueMeCommand, callbackData, queue.Id) }
            : new InlineKeyboardButton[] { GetQueueRelatedButton(MessageProvider.GetMessage(CallbackMessageKeys.GetQueueCallbackHandler.GetQueueCallback_EnqueueMe_Button), CallbackConstants.EnqueueCommand, callbackData, queue.Id) }
        };

        if (queue.IsQueueCreator(user) || await TelegramBotClient.IsChatAdmin(user.Id, queue.GroupId))
        {
            replyMarkupButtons.Add(new InlineKeyboardButton[] 
            {
                GetRemoveQueueButton(MessageProvider.GetMessage(CallbackMessageKeys.GetQueueCallbackHandler.GetQueueCallback_RemoveQueue_Button), callbackData),
                GetDynamicQueueButton(CallbackConstants.SwitchQueueDynamicCommand, callbackData, queue)
            });
        }

        if (doesUserParticipate)
        {
            replyMarkupButtons.Add(new InlineKeyboardButton[] { GetQueueRelatedButton("Exchange positions", CallbackConstants.ExchangePositionsCommand, callbackData, queue.Id) });
        }

        replyMarkupButtons.Add(new InlineKeyboardButton[] { GetRefreshButton(callbackData) });
        replyMarkupButtons.Add(new InlineKeyboardButton[] { GetReturnToChatButton(callbackData) });
        return new InlineKeyboardMarkup(replyMarkupButtons);
    }

    private InlineKeyboardButton GetQueueRelatedButton(string buttonText, string command, CallbackData callbackData, int queueId)
    {
        var buttonCallbackData = new CallbackData()
        {
            Command = command,
            TargetChatId = callbackData.TargetChatId,
            QueueData = new QueueData()
            { 
                QueueId = queueId,
            }
        };

        var serializedCallbackData = DataSerializer.Serialize(buttonCallbackData);
        return InlineKeyboardButton.WithCallbackData(buttonText, serializedCallbackData);
    }

    private InlineKeyboardButton GetDynamicQueueButton(string command, CallbackData callbackData, Queue queue)
    {
        var buttonCallbackData = new CallbackData()
        {
            Command = command,
            TargetChatId = callbackData.TargetChatId,
            QueueData = new QueueData()
            {
                QueueId = queue.Id,
            }
        };

        var buttonText = queue.IsDynamic 
            ? MessageProvider.GetMessage(CallbackMessageKeys.GetQueueCallbackHandler.GetQueueCallback_MakeQueueStatic_Button)
            : MessageProvider.GetMessage(CallbackMessageKeys.GetQueueCallbackHandler.GetQueueCallback_MakeQueueDynamic_Button);

        var serializedCallbackData = DataSerializer.Serialize(buttonCallbackData);
        return InlineKeyboardButton.WithCallbackData(buttonText, serializedCallbackData);
    }

    private string BuildResponseMessage(Queue queue)
    {
        if (!queue.Members.Any())
        {
            if (queue.IsDynamic)
            {
                return MessageProvider.GetMessage(CallbackMessageKeys.GetQueueCallbackHandler.GetQueueCallback_ListQueueMembers_QueueIsEmpty_Message, queue.Name)
                    + MessageProvider.GetMessage(CallbackMessageKeys.GetQueueCallbackHandler.GetQueueCallback_ListQueueMembers_QueueIsDynamic_PostScriptum_Message);
            }

            return MessageProvider.GetMessage(CallbackMessageKeys.GetQueueCallbackHandler.GetQueueCallback_ListQueueMembers_QueueIsEmpty_Message, queue.Name);
        }

        var responseMessage = new StringBuilder(MessageProvider.GetMessage(CallbackMessageKeys.GetQueueCallbackHandler.GetQueueCallback_ListQueueMembers_Message, queue.Name));
        foreach (var queueParticipant in queue.Members.OrderBy(userInQueue => userInQueue.Position))
        {
            responseMessage.AppendLine($"{queueParticipant.Position}) <b>{queueParticipant.User.FullName}</b>");
        }

        if (queue.IsDynamic)
        {
            responseMessage.AppendLine(MessageProvider.GetMessage(CallbackMessageKeys.GetQueueCallbackHandler.GetQueueCallback_ListQueueMembers_QueueIsDynamic_PostScriptum_Message));
        }

        return responseMessage.ToString();
    }
}
