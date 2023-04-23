using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Enqueuer.Persistence.Extensions;
using Enqueuer.Services;
using Enqueuer.Telegram.Callbacks.CallbackHandlers.BaseClasses;
using Enqueuer.Telegram.Callbacks.Extensions;
using Enqueuer.Telegram.Core;
using Enqueuer.Telegram.Core.Constants;
using Enqueuer.Telegram.Core.Localization;
using Enqueuer.Telegram.Core.Serialization;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Queue = Enqueuer.Persistence.Models.Queue;
using User = Enqueuer.Persistence.Models.User;

namespace Enqueuer.Telegram.Callbacks.CallbackHandlers;

public class GetQueueCallbackHandler : CallbackHandlerBaseWithRemoveQueueButton
{
    private readonly IQueueService _queueService;
    private readonly IUserService _userService;

    public GetQueueCallbackHandler(ITelegramBotClient telegramBotClient, IQueueService queueService, IUserService userService, ICallbackDataSerializer dataSerializer, ILocalizationProvider localizationProvider)
        : base(telegramBotClient, dataSerializer, localizationProvider)
    {
        _queueService = queueService;
        _userService = userService;
    }

    protected override Task HandleAsyncImplementation(Callback callback, CancellationToken cancellationToken)
    {
        if (callback.CallbackData?.QueueData == null)
        {
            return TelegramBotClient.EditMessageTextAsync(
                callback.Message.Chat,
                callback.Message.MessageId,
                LocalizationProvider.GetMessage(CallbackMessageKeys.Callback_OutdatedCallback_Message, MessageParameters.None),
                ParseMode.Html,
                cancellationToken: cancellationToken);
        }

        return HandleAsyncInternal(callback, cancellationToken);
    }

    private async Task HandleAsyncInternal(Callback callback, CancellationToken cancellationToken)
    {
        var queue = await _queueService.GetQueueAsync(callback.CallbackData!.QueueData.QueueId, includeMembers: true, cancellationToken);
        if (queue == null)
        {
            var returnButton = GetReturnToChatButton(callback.CallbackData);
            await TelegramBotClient.EditMessageTextAsync(
                callback.Message.Chat,
                callback.Message.MessageId,
                LocalizationProvider.GetMessage(CallbackMessageKeys.Callback_QueueHasBeenDeleted_Message, MessageParameters.None),
                replyMarkup: returnButton,
                cancellationToken: cancellationToken);

            return;
        }

        await HandleCallbackWithExistingQueueAsync(callback, queue, cancellationToken);
    }

    private async Task HandleCallbackWithExistingQueueAsync(Callback callback, Queue queue, CancellationToken cancellationToken)
    {
        var user = await _userService.GetOrStoreUserAsync(callback.From, cancellationToken);

        var replyMarkup = await BuildReplyMarkup(user, queue, callback.CallbackData!);
        var responseMessage = BuildResponseMessage(queue);

        await TelegramBotClient.EditMessageTextAsync(
            callback.Message.Chat,
            callback.Message.MessageId,
            responseMessage,
            ParseMode.Html,
            replyMarkup: replyMarkup,
            cancellationToken: cancellationToken);
    }

    private async Task<InlineKeyboardMarkup> BuildReplyMarkup(User user, Queue queue, CallbackData callbackData)
    {
        var doesUserParticipate = user.IsParticipatingIn(queue);
        var replyMarkupButtons = new List<InlineKeyboardButton[]>()
        {
            doesUserParticipate
            ? new InlineKeyboardButton[] { GetQueueRelatedButton(LocalizationProvider.GetMessage(CallbackMessageKeys.GetQueueCallbackHandler.Callback_GetQueue_DequeueMe_Button, MessageParameters.None), CallbackCommands.DequeueMeCommand, callbackData, queue.Id) }
            : new InlineKeyboardButton[] { GetQueueRelatedButton(LocalizationProvider.GetMessage(CallbackMessageKeys.GetQueueCallbackHandler.Callback_GetQueue_EnqueueMe_Button, MessageParameters.None), CallbackCommands.EnqueueCommand, callbackData, queue.Id) }
        };

        if (queue.IsQueueCreator(user) || await TelegramBotClient.IsChatAdmin(user.Id, queue.GroupId))
        {
            replyMarkupButtons.Add(new InlineKeyboardButton[]
            {
                GetRemoveQueueButton(LocalizationProvider.GetMessage(CallbackMessageKeys.GetQueueCallbackHandler.Callback_GetQueue_RemoveQueue_Button, MessageParameters.None), callbackData),
                GetDynamicQueueButton(CallbackCommands.SwitchQueueDynamicCommand, callbackData, queue)
            });
        }

        if (doesUserParticipate)
        {
            replyMarkupButtons.Add(new InlineKeyboardButton[] { GetQueueRelatedButton(LocalizationProvider.GetMessage(CallbackMessageKeys.GetQueueCallbackHandler.Callback_GetQueue_SwapPositions_Button, MessageParameters.None), CallbackCommands.ExchangePositionsCommand, callbackData, queue.Id) });
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
            ? LocalizationProvider.GetMessage(CallbackMessageKeys.GetQueueCallbackHandler.Callback_GetQueue_MakeQueueStatic_Button, MessageParameters.None)
            : LocalizationProvider.GetMessage(CallbackMessageKeys.GetQueueCallbackHandler.Callback_GetQueue_MakeQueueDynamic_Button, MessageParameters.None);

        var serializedCallbackData = DataSerializer.Serialize(buttonCallbackData);
        return InlineKeyboardButton.WithCallbackData(buttonText, serializedCallbackData);
    }

    private string BuildResponseMessage(Queue queue)
    {
        if (!queue.Members.Any())
        {
            if (queue.IsDynamic)
            {
                return LocalizationProvider.GetMessage(CallbackMessageKeys.GetQueueCallbackHandler.Callback_GetQueue_ListQueueMembers_QueueIsEmpty_Message, new MessageParameters(queue.Name))
                    + LocalizationProvider.GetMessage(CallbackMessageKeys.GetQueueCallbackHandler.Callback_GetQueue_ListQueueMembers_QueueIsDynamic_PostScriptum_Message, MessageParameters.None);
            }

            return LocalizationProvider.GetMessage(CallbackMessageKeys.GetQueueCallbackHandler.Callback_GetQueue_ListQueueMembers_QueueIsEmpty_Message, new MessageParameters(queue.Name));
        }

        var responseMessage = new StringBuilder(LocalizationProvider.GetMessage(CallbackMessageKeys.GetQueueCallbackHandler.Callback_GetQueue_ListQueueMembers_Message, new MessageParameters(queue.Name)));
        foreach (var queueParticipant in queue.Members.OrderBy(userInQueue => userInQueue.Position))
        {
            responseMessage.AppendLine($"{queueParticipant.Position}) <b>{queueParticipant.User.FullName}</b>");
        }

        if (queue.IsDynamic)
        {
            responseMessage.AppendLine(LocalizationProvider.GetMessage(CallbackMessageKeys.GetQueueCallbackHandler.Callback_GetQueue_ListQueueMembers_QueueIsDynamic_PostScriptum_Message, MessageParameters.None));
        }

        return responseMessage.ToString();
    }
}
