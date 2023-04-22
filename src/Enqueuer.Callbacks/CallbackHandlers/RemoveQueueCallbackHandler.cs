using System;
using System.Threading;
using System.Threading.Tasks;
using Enqueuer.Callbacks.CallbackHandlers.BaseClasses;
using Enqueuer.Callbacks.Extensions;
using Enqueuer.Persistence.Extensions;
using Enqueuer.Persistence.Models;
using Enqueuer.Services;
using Enqueuer.Telegram.Core.Localization;
using Enqueuer.Telegram.Core.Serialization;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Enqueuer.Callbacks.CallbackHandlers;

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
        var queue = await _queueService.GetQueueAsync(callback.CallbackData!.QueueData!.QueueId, includeMembers: false, cancellationToken);
        if (queue == null)
        {
            await TelegramBotClient.EditMessageTextAsync(
                callback.Message.Chat,
                callback.Message.MessageId,
                LocalizationProvider.GetMessage(CallbackMessageKeys.RemoveQueueCallbackHandler.Callback_RemoveQueue_QueueHasBeenDeleted_Message, MessageParameters.None),
                replyMarkup: GetReturnToChatButton(callback.CallbackData),
                cancellationToken: cancellationToken);

            return;
        }

        await HandleCallbackWithExistingQueueAsync(queue, callback, cancellationToken);
    }

    private async Task HandleCallbackWithExistingQueueAsync(Queue queue, Callback callback, CancellationToken cancellationToken)
    {
        if (callback.CallbackData.HasUserAgreement)
        {
            await HandleCallbackWithUserAgreementAsync(callback, queue, cancellationToken);
            return;
        }

        var replyMarkup = new InlineKeyboardButton[][]
        {
            new InlineKeyboardButton[] { GetRemoveQueueButton(LocalizationProvider.GetMessage(CallbackMessageKeys.RemoveQueueCallbackHandler.Callback_RemoveQueue_AgreeToDelete_Button, MessageParameters.None), callback.CallbackData, true) },
            new InlineKeyboardButton[] { GetReturnToQueueButton(callback.CallbackData) }
        };

        await TelegramBotClient.EditMessageTextAsync(
            callback.Message.Chat,
            callback.Message.MessageId,
            LocalizationProvider.GetMessage(CallbackMessageKeys.RemoveQueueCallbackHandler.Callback_RemoveQueue_AreYouSureToDeleteQueue_Message, new MessageParameters(queue.Name)),
            ParseMode.Html,
            replyMarkup: replyMarkup,
            cancellationToken: cancellationToken);
    }

    private async Task HandleCallbackWithUserAgreementAsync(Callback callback, Queue queue, CancellationToken cancellationToken)
    {
        var user = await _userService.GetOrStoreUserAsync(callback.From, cancellationToken);
        if (callback.CallbackData.HasUserAgreement)
        {
            var userId = callback.From.Id;
            if (!queue.IsQueueCreator(userId) && !await TelegramBotClient.IsChatAdmin(userId, queue.GroupId))
            {
                await TelegramBotClient.EditMessageTextAsync(
                    callback.Message.Chat,
                    callback.Message.MessageId,
                    LocalizationProvider.GetMessage(CallbackMessageKeys.RemoveQueueCallbackHandler.Callback_RemoveQueue_UserHasNoRightsToDelete_Message, new MessageParameters(queue.Name)),
                    ParseMode.Html,
                    replyMarkup: GetReturnToQueueButton(callback.CallbackData),
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
                callback.Message.Chat,
                callback.Message.MessageId,
                LocalizationProvider.GetMessage(CallbackMessageKeys.RemoveQueueCallbackHandler.Callback_RemoveQueue_Success_Message, new MessageParameters(queue.Name)),
                ParseMode.Html,
                replyMarkup: GetReturnToChatButton(callback.CallbackData),
                cancellationToken: cancellationToken);

            return;
        }

        throw new Exception("False 'IsUserAgreed' value passed to message handler.");
    }
}
