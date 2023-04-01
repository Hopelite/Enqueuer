using System.Threading;
using System.Threading.Tasks;
using Enqueuer.Callbacks.CallbackHandlers.BaseClasses;
using Enqueuer.Callbacks.Exceptions;
using Enqueuer.Callbacks.Extensions;
using Enqueuer.Data.DataSerialization;
using Enqueuer.Data.TextProviders;
using Enqueuer.Persistence.Extensions;
using Enqueuer.Persistence.Models;
using Enqueuer.Services;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Enqueuer.Callbacks.CallbackHandlers;

public class RemoveQueueCallbackHandler : CallbackHandlerBaseWithRemoveQueueButton
{
    private readonly IUserService _userService;
    private readonly IQueueService _queueService;
    private readonly ILogger<DequeueMeCallbackHandler> _logger;

    public RemoveQueueCallbackHandler(ITelegramBotClient telegramBotClient, IDataSerializer dataSerializer, IMessageProvider messageProvider, IUserService userService, IQueueService queueService, ILogger<DequeueMeCallbackHandler> logger)
        : base(telegramBotClient, dataSerializer, messageProvider)
    {
        _userService = userService;
        _queueService = queueService;
        _logger = logger;
    }

    protected override Task HandleAsyncImplementation(Callback callback)
    {
        if (callback.CallbackData?.QueueData == null)
        {
            _logger.LogWarning("Handled outdated callback.");
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
        var queue = await _queueService.GetQueueAsync(callback.CallbackData!.QueueData.QueueId, includeMembers: false, CancellationToken.None);
        if (queue == null)
        {
            await TelegramBotClient.EditMessageTextAsync(
                callback.Message.Chat,
                callback.Message.MessageId,
                MessageProvider.GetMessage(CallbackMessageKeys.RemoveQueueCallbackHandler.RemoveQueueCallback_QueueHasBeenDeleted_Message),
                replyMarkup: GetReturnToChatButton(callback.CallbackData));

            return;
        }

        await HandleCallbackWithExistingQueueAsync(queue, callback);
    }

    private async Task HandleCallbackWithExistingQueueAsync(Queue queue, Callback callback)
    {
        if (callback.CallbackData.HasUserAgreement)
        {
            await HandleCallbackWithUserAgreementAsync(callback, queue);
            return;
        }

        var replyMarkup = new InlineKeyboardButton[][]
        {
            new InlineKeyboardButton[] { GetRemoveQueueButton(MessageProvider.GetMessage(CallbackMessageKeys.RemoveQueueCallbackHandler.RemoveQueueCallback_AgreeToDelete_Button), callback.CallbackData, true) },
            new InlineKeyboardButton[] { GetReturnToQueueButton(callback.CallbackData) }
        };

        await TelegramBotClient.EditMessageTextAsync(
            callback.Message.Chat,
            callback.Message.MessageId,
            MessageProvider.GetMessage(CallbackMessageKeys.RemoveQueueCallbackHandler.RemoveQueueCallback_AreYouSureToDeleteQueue_Message, queue.Name),
            ParseMode.Html,
            replyMarkup: replyMarkup);
    }

    private async Task<Message> HandleCallbackWithUserAgreementAsync(Callback callback, Queue queue)
    {
        var user = await _userService.GetOrStoreUserAsync(callback.From, CancellationToken.None);
        if (callback.CallbackData.QueueData.IsUserAgreed!.Value)
        {
            var userId = callback.From.Id;
            if (!queue.IsQueueCreator(userId) && !await TelegramBotClient.IsChatAdmin(userId, queue.GroupId))
            {
                return await TelegramBotClient.EditMessageTextAsync(
                    callback.Message.Chat,
                    callback.Message.MessageId,
                    MessageProvider.GetMessage(CallbackMessageKeys.RemoveQueueCallbackHandler.RemoveQueueCallback_UserHasNoRightsToDelete_Message, queue.Name),
                    ParseMode.Html,
                    replyMarkup: GetReturnToQueueButton(callback.CallbackData));
            }

            await _queueService.DeleteQueueAsync(queue, CancellationToken.None);

            await TelegramBotClient.SendTextMessageAsync(
                queue.GroupId,
                MessageProvider.GetMessage(CallbackMessageKeys.RemoveQueueCallbackHandler.RemoveQueueCallback_Success_PublicChat_Message, user.FullName, queue.Name),
                ParseMode.Html);

            return await TelegramBotClient.EditMessageTextAsync(
                callback.Message.Chat,
                callback.Message.MessageId,
                MessageProvider.GetMessage(CallbackMessageKeys.RemoveQueueCallbackHandler.RemoveQueueCallback_Success_Message, queue.Name),
                ParseMode.Html,
                replyMarkup: GetReturnToChatButton(callback.CallbackData));
        }

        throw new CallbackMessageHandlingException("False 'IsUserAgreed' value passed to message handler.");
    }
}
