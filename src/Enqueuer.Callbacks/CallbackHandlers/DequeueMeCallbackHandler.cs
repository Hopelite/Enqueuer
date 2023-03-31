using System.Threading;
using System.Threading.Tasks;
using Enqueuer.Callbacks.CallbackHandlers.BaseClasses;
using Enqueuer.Data.Constants;
using Enqueuer.Data;
using Enqueuer.Data.DataSerialization;
using Enqueuer.Data.TextProviders;
using Enqueuer.Persistence.Models;
using Enqueuer.Services;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Enqueuer.Callbacks.CallbackHandlers;

public class DequeueMeCallbackHandler : CallbackHandlerBaseWithReturnToQueueButton
{
    private readonly IUserService _userService;
    private readonly IQueueService _queueService;
    private readonly ILogger<DequeueMeCallbackHandler> _logger;

    public DequeueMeCallbackHandler(ITelegramBotClient telegramBotClient, IDataSerializer dataSerializer, IMessageProvider messageProvider, IUserService userService, IQueueService queueService, ILogger<DequeueMeCallbackHandler> logger)
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
                MessageProvider.GetMessage(CallbackMessageKeys.QueueHasBeenDeleted_Message),
                replyMarkup: GetReturnToChatButton(callback.CallbackData));

            return;
        }

        await HandleCallbackWithExistingQueueAsync(queue, callback);
    }

    private async Task HandleCallbackWithExistingQueueAsync(Queue queue, Callback callback)
    {
        if (callback.CallbackData.HasUserAgreement)
        {
            await HandleCallbackWithUserAgreementAsync(queue, callback);
            return;
        }

        var replyMarkup = new InlineKeyboardButton[][]
        {
            new InlineKeyboardButton[] { GetDequeueButton(MessageProvider.GetMessage(CallbackMessageKeys.DequeueMeCallbackHandler.DequeueMeCallback_AgreeToDequeue_Button), callback.CallbackData, true) },
            new InlineKeyboardButton[] { GetReturnToQueueButton(callback.CallbackData) }
        };

        await TelegramBotClient.EditMessageTextAsync(
            callback.Message.Chat,
            callback.Message.MessageId,
            MessageProvider.GetMessage(CallbackMessageKeys.DequeueMeCallbackHandler.DequeueMeCallback_AreYouSureToBeDequeued_Message, queue.Name),
            ParseMode.Html,
            replyMarkup: replyMarkup);
    }

    private async Task HandleCallbackWithUserAgreementAsync(Queue queue, Callback callback)
    {
        var user = await _userService.GetOrStoreUserAsync(callback.From, CancellationToken.None);
        var responseMessage = await _queueService.TryDequeueUserAsync(user, queue.Id, CancellationToken.None)
            ? MessageProvider.GetMessage(CallbackMessageKeys.DequeueMeCallbackHandler.DequeueMeCallback_Success_Message, queue.Name)
            : MessageProvider.GetMessage(CallbackMessageKeys.DequeueMeCallbackHandler.DequeueMeCallback_UserDoesNotParticipate_Message, queue.Name);

        await TelegramBotClient.EditMessageTextAsync(
            callback.Message.Chat,
            callback.Message.MessageId,
            responseMessage,
            ParseMode.Html,
            replyMarkup: GetReturnToQueueButton(callback.CallbackData!));
    }

    private InlineKeyboardButton GetDequeueButton(string buttonText, CallbackData callbackData, bool? isAgreed = null)
    {
        var buttonCallbackData = new CallbackData()
        {
            Command = CallbackConstants.DequeueMeCommand,
            TargetChatId = callbackData.TargetChatId,
            UserAgreement = isAgreed,
            QueueData = new QueueData()
            {
                QueueId = callbackData.QueueData.QueueId,
            }
        };

        var serializedCallbackData = DataSerializer.Serialize(buttonCallbackData);
        return InlineKeyboardButton.WithCallbackData(buttonText, serializedCallbackData);
    }
}
