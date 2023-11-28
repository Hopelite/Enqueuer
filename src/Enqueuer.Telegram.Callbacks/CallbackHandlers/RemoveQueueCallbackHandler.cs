using System;
using System.Threading;
using System.Threading.Tasks;
using Enqueuer.Persistence.Extensions;
using Enqueuer.Persistence.Models;
using Enqueuer.Services;
using Enqueuer.Telegram.Callbacks.CallbackHandlers.BaseClasses;
using Enqueuer.Telegram.Callbacks.Extensions;
using Enqueuer.Messaging.Core.Localization;
using Enqueuer.Messaging.Core.Serialization;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Enqueuer.Messaging.Core.Types.Callbacks;

namespace Enqueuer.Telegram.Callbacks.CallbackHandlers;

public class RemoveQueueCallbackHandler : CallbackHandlerBaseWithRemoveQueueButton
{
    private readonly IUserService _userService;
    private readonly IQueueService _queueService;

    public RemoveQueueCallbackHandler(
        ITelegramBotClient telegramBotClient, ICallbackDataSerializer dataSerializer,
        ILocalizationProvider localizationProvider, IUserService userService, IQueueService queueService)
        : base(telegramBotClient, dataSerializer, localizationProvider)
    {
        _userService = userService;
        _queueService = queueService;
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
        var queue = await _queueService.GetQueueAsync(callbackContext.CallbackData!.QueueData!.QueueId, includeMembers: false, cancellationToken);
        if (queue == null)
        {
            await TelegramBotClient.EditMessageTextAsync(
                callbackContext.Chat.Id,
                callbackContext.MessageId,
                LocalizationProvider.GetMessage(CallbackMessageKeys.RemoveQueueCallbackHandler.Callback_RemoveQueue_QueueHasBeenDeleted_Message, MessageParameters.None),
                replyMarkup: GetReturnToChatButton(callbackContext.CallbackData),
                cancellationToken: cancellationToken);

            return;
        }

        await HandleCallbackWithExistingQueueAsync(queue, callbackContext, cancellationToken);
    }

    private async Task HandleCallbackWithExistingQueueAsync(Queue queue, CallbackContext callbackContext, CancellationToken cancellationToken)
    {
        if (callbackContext.CallbackData.HasUserAgreement)
        {
            await HandleCallbackWithUserAgreementAsync(callbackContext, queue, cancellationToken);
            return;
        }

        var replyMarkup = new InlineKeyboardButton[][]
        {
            new InlineKeyboardButton[] { GetRemoveQueueButton(LocalizationProvider.GetMessage(CallbackMessageKeys.RemoveQueueCallbackHandler.Callback_RemoveQueue_AgreeToDelete_Button, MessageParameters.None), callbackContext.CallbackData, true) },
            new InlineKeyboardButton[] { GetReturnToQueueButton(callbackContext.CallbackData) }
        };

        await TelegramBotClient.EditMessageTextAsync(
            callbackContext.Chat.Id,
            callbackContext.MessageId,
            LocalizationProvider.GetMessage(CallbackMessageKeys.RemoveQueueCallbackHandler.Callback_RemoveQueue_AreYouSureToDeleteQueue_Message, new MessageParameters(queue.Name)),
            ParseMode.Html,
            replyMarkup: replyMarkup,
            cancellationToken: cancellationToken);
    }

    private async Task HandleCallbackWithUserAgreementAsync(CallbackContext callbackContext, Queue queue, CancellationToken cancellationToken)
    {
        var user = await _userService.GetOrStoreUserAsync(callbackContext.Sender, cancellationToken);
        if (callbackContext.CallbackData.HasUserAgreement)
        {
            var userId = callbackContext.Sender.Id;
            if (!queue.IsQueueCreator(userId) && !await TelegramBotClient.IsChatAdmin(userId, queue.GroupId))
            {
                await TelegramBotClient.EditMessageTextAsync(
                callbackContext.Chat.Id,
                callbackContext.MessageId,
                    LocalizationProvider.GetMessage(CallbackMessageKeys.RemoveQueueCallbackHandler.Callback_RemoveQueue_UserHasNoRightsToDelete_Message, new MessageParameters(queue.Name)),
                    ParseMode.Html,
                    replyMarkup: GetReturnToQueueButton(callbackContext.CallbackData),
                    cancellationToken: cancellationToken);
                return;
            }

            await _queueService.DeleteQueueAsync(queue, cancellationToken);

            await TelegramBotClient.SendTextMessageAsync(
                queue.GroupId,
                LocalizationProvider.GetMessage(CallbackMessageKeys.RemoveQueueCallbackHandler.Callback_RemoveQueue_Success_PublicChat_Message, new MessageParameters(user.FullName, queue.Name)),
                ParseMode.Html,
                cancellationToken: cancellationToken);

            await TelegramBotClient.EditMessageTextAsync(
                callbackContext.Chat.Id,
                callbackContext.MessageId,
                LocalizationProvider.GetMessage(CallbackMessageKeys.RemoveQueueCallbackHandler.Callback_RemoveQueue_Success_Message, new MessageParameters(queue.Name)),
                ParseMode.Html,
                replyMarkup: GetReturnToChatButton(callbackContext.CallbackData),
                cancellationToken: cancellationToken);

            return;
        }

        throw new Exception("False 'IsUserAgreed' value passed to message handler.");
    }
}
