using System;
using System.Threading;
using System.Threading.Tasks;
using Enqueuer.Messaging.Core.Localization;
using Enqueuer.Messaging.Core.Serialization;
using Enqueuer.Messaging.Core.Types.Callbacks;
using Enqueuer.Persistence.Extensions;
using Enqueuer.Persistence.Models;
using Enqueuer.Services;
using Enqueuer.Telegram.Callbacks.Extensions;
using Enqueuer.Telegram.Callbacks.Helpers;
using Enqueuer.Telegram.Callbacks.Helpers.Markup;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace Enqueuer.Telegram.Callbacks.CallbackHandlers;

public class RemoveQueueCallbackHandler : CallbackHandlerBase
{
    private readonly ICallbackDataSerializer _dataSerializer;
    private readonly IUserService _userService;
    private readonly IQueueService _queueService;

    public RemoveQueueCallbackHandler(
        ITelegramBotClient telegramBotClient, ICallbackDataSerializer dataSerializer,
        ILocalizationProvider localizationProvider, IUserService userService, IQueueService queueService)
        : base(telegramBotClient, localizationProvider)
    {
        _dataSerializer = dataSerializer;
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
            var replyMarkup = new ReturnToChatMarkup(_dataSerializer, LocalizationProvider)
                .Create(callbackContext.CallbackData);

            await TelegramBotClient.EditMessageTextAsync(
                callbackContext.Chat.Id,
                callbackContext.MessageId,
                LocalizationProvider.GetMessage(CallbackMessageKeys.RemoveQueueCallbackHandler.Callback_RemoveQueue_QueueHasBeenDeleted_Message, MessageParameters.None),
                replyMarkup: replyMarkup,
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

        var replyMarkup = ReplyMarkupBuilder.Create(_dataSerializer, LocalizationProvider)
            .WithRemoveQueueButton(
                LocalizationProvider.GetMessage(CallbackMessageKeys.RemoveQueueCallbackHandler.Callback_RemoveQueue_AgreeToDelete_Button, MessageParameters.None),
                callbackContext.CallbackData,
                isAgreed: true)
            .FromNewRow()
                .WithReturnToQueueButton(callbackContext.CallbackData)
            .Build();

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
            var replyMarkup = ReplyMarkupBuilder.Create(_dataSerializer, LocalizationProvider);

            var userId = callbackContext.Sender.Id;
            if (!queue.IsQueueCreator(userId) && !await TelegramBotClient.IsChatAdmin(userId, queue.GroupId))
            {
                replyMarkup.WithReturnToQueueButton(callbackContext.CallbackData);

                await TelegramBotClient.EditMessageTextAsync(
                callbackContext.Chat.Id,
                callbackContext.MessageId,
                    LocalizationProvider.GetMessage(CallbackMessageKeys.RemoveQueueCallbackHandler.Callback_RemoveQueue_UserHasNoRightsToDelete_Message, new MessageParameters(queue.Name)),
                    ParseMode.Html,
                    replyMarkup: replyMarkup.Build(),
                    cancellationToken: cancellationToken);
                return;
            }

            await _queueService.DeleteQueueAsync(queue, cancellationToken);

            await TelegramBotClient.SendTextMessageAsync(
                queue.GroupId,
                LocalizationProvider.GetMessage(CallbackMessageKeys.RemoveQueueCallbackHandler.Callback_RemoveQueue_Success_PublicChat_Message, new MessageParameters(user.FullName, queue.Name)),
                ParseMode.Html,
                cancellationToken: cancellationToken);

            replyMarkup.WithReturnToChatButton(callbackContext.CallbackData);

            await TelegramBotClient.EditMessageTextAsync(
                callbackContext.Chat.Id,
                callbackContext.MessageId,
                LocalizationProvider.GetMessage(CallbackMessageKeys.RemoveQueueCallbackHandler.Callback_RemoveQueue_Success_Message, new MessageParameters(queue.Name)),
                ParseMode.Html,
                replyMarkup: replyMarkup.Build(),
                cancellationToken: cancellationToken);

            return;
        }

        throw new Exception("False 'IsUserAgreed' value passed to message handler.");
    }
}
