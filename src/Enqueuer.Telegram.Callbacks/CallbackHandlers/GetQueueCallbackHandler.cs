using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Enqueuer.Messaging.Core;
using Enqueuer.Messaging.Core.Constants;
using Enqueuer.Messaging.Core.Localization;
using Enqueuer.Messaging.Core.Serialization;
using Enqueuer.Messaging.Core.Types.Callbacks;
using Enqueuer.Persistence.Extensions;
using Enqueuer.Services;
using Enqueuer.Telegram.Callbacks.Extensions;
using Enqueuer.Telegram.Callbacks.Helpers;
using Enqueuer.Telegram.Callbacks.Helpers.Markup;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Queue = Enqueuer.Persistence.Models.Queue;
using User = Enqueuer.Persistence.Models.User;

namespace Enqueuer.Telegram.Callbacks.CallbackHandlers;

public class GetQueueCallbackHandler : CallbackHandlerBase
{
    private readonly IQueueService _queueService;
    private readonly IUserService _userService;
    private readonly ICallbackDataSerializer _dataSerializer;

    public GetQueueCallbackHandler(ITelegramBotClient telegramBotClient, IQueueService queueService, IUserService userService, ICallbackDataSerializer dataSerializer, ILocalizationProvider localizationProvider)
        : base(telegramBotClient, localizationProvider)
    {
        _queueService = queueService;
        _userService = userService;
        _dataSerializer = dataSerializer;
    }

    protected override Task HandleAsyncImplementation(CallbackContext callbackContext, CancellationToken cancellationToken)
    {
        if (callbackContext.CallbackData?.QueueData == null)
        {
            return TelegramBotClient.EditMessageTextAsync(
                callbackContext.Chat.Id,
                callbackContext.MessageId,
                LocalizationProvider.GetMessage(CallbackMessageKeys.Callback_OutdatedCallback_Message, MessageParameters.None),
                ParseMode.Html,
                cancellationToken: cancellationToken);
        }

        return HandleAsyncInternal(callbackContext, cancellationToken);
    }

    private async Task HandleAsyncInternal(CallbackContext callbackContext, CancellationToken cancellationToken)
    {
        var queue = await _queueService.GetQueueAsync(callbackContext.CallbackData.QueueData!.QueueId, includeMembers: true, cancellationToken);
        if (queue == null)
        {
            var replyMarkup = new ReturnToChatMarkup(_dataSerializer, LocalizationProvider)
                .Create(callbackContext.CallbackData);

            await TelegramBotClient.EditMessageTextAsync(
                callbackContext.Chat.Id,
                callbackContext.MessageId,
                LocalizationProvider.GetMessage(CallbackMessageKeys.Callback_QueueHasBeenDeleted_Message, MessageParameters.None),
                replyMarkup: replyMarkup,
                cancellationToken: cancellationToken);

            return;
        }

        await HandleCallbackWithExistingQueueAsync(callbackContext, queue, cancellationToken);
    }

    private async Task HandleCallbackWithExistingQueueAsync(CallbackContext callbackContext, Queue queue, CancellationToken cancellationToken)
    {
        var user = await _userService.GetOrStoreUserAsync(callbackContext.Sender, cancellationToken);

        var replyMarkup = await BuildReplyMarkup(user, queue, callbackContext.CallbackData!);
        var responseMessage = BuildResponseMessage(queue);

        await TelegramBotClient.EditMessageTextAsync(
            callbackContext.Chat.Id,
            callbackContext.MessageId,
            responseMessage,
            ParseMode.Html,
            replyMarkup: replyMarkup,
            cancellationToken: cancellationToken);
    }

    private async Task<InlineKeyboardMarkup> BuildReplyMarkup(User user, Queue queue, CallbackData callbackData)
    {
        var doesUserParticipate = user.IsParticipatingIn(queue);

        var replyMarkup = ReplyMarkupBuilder.Create(_dataSerializer, LocalizationProvider);
        if (doesUserParticipate)
        {
            replyMarkup.WithQueueRelatedButton(
                LocalizationProvider.GetMessage(CallbackMessageKeys.GetQueueCallbackHandler.Callback_GetQueue_DequeueMe_Button, MessageParameters.None),
                CallbackCommands.DequeueMeCommand,
                callbackData,
                queue.Id);
        }
        else
        {
            replyMarkup.WithQueueRelatedButton(
                LocalizationProvider.GetMessage(CallbackMessageKeys.GetQueueCallbackHandler.Callback_GetQueue_EnqueueMe_Button, MessageParameters.None),
                CallbackCommands.EnqueueCommand,
                callbackData,
                queue.Id);
        }

        if (queue.IsQueueCreator(user) || await TelegramBotClient.IsChatAdmin(user.Id, queue.GroupId))
        {
            replyMarkup.FromNewRow()
                .WithRemoveQueueButton(LocalizationProvider.GetMessage(CallbackMessageKeys.GetQueueCallbackHandler.Callback_GetQueue_RemoveQueue_Button, MessageParameters.None), callbackData)
                .WithDynamicQueueButton(CallbackCommands.SwitchQueueDynamicCommand, callbackData, queue);
        }

        if (doesUserParticipate)
        {
            replyMarkup.FromNewRow()
                .WithQueueRelatedButton(
                    LocalizationProvider.GetMessage(CallbackMessageKeys.GetQueueCallbackHandler.Callback_GetQueue_SwapPositions_Button, MessageParameters.None),
                    CallbackCommands.ExchangePositionsCommand,
                    callbackData,
                    queue.Id);
        }

        return replyMarkup.FromNewRow()
            .WithRefreshButton(callbackData)
            .FromNewRow()
            .WithReturnToChatButton(callbackData)
            .Build();
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
        responseMessage.Append(Environment.NewLine);

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
