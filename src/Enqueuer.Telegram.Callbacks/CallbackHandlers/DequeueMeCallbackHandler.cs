using System.Threading;
using System.Threading.Tasks;
using Enqueuer.Persistence.Models;
using Enqueuer.Services;
using Enqueuer.Telegram.Callbacks.CallbackHandlers.BaseClasses;
using Enqueuer.Messaging.Core;
using Enqueuer.Messaging.Core.Constants;
using Enqueuer.Messaging.Core.Localization;
using Enqueuer.Messaging.Core.Serialization;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Enqueuer.Messaging.Core.Types.Callbacks;

namespace Enqueuer.Telegram.Callbacks.CallbackHandlers;

public class DequeueMeCallbackHandler : CallbackHandlerBaseWithReturnToQueueButton
{
    private readonly IUserService _userService;
    private readonly IQueueService _queueService;
    private readonly ILogger<DequeueMeCallbackHandler> _logger;

    public DequeueMeCallbackHandler(ITelegramBotClient telegramBotClient, ICallbackDataSerializer dataSerializer, ILocalizationProvider localizationProvider, IUserService userService, IQueueService queueService, ILogger<DequeueMeCallbackHandler> logger)
        : base(telegramBotClient, dataSerializer, localizationProvider)
    {
        _userService = userService;
        _queueService = queueService;
        _logger = logger;
    }

    protected override Task HandleAsyncImplementation(CallbackContext callbackContext, CancellationToken cancellationToken)
    {
        if (callbackContext.CallbackData.QueueData == null)
        {
            _logger.LogWarning("Handled outdated callback.");
            return TelegramBotClient.EditMessageTextAsync(
                callbackContext.Chat.Id,
                callbackContext.MessageId,
                LocalizationProvider.GetMessage(CallbackMessageKeys.Callback_OutdatedCallback_Message, MessageParameters.None),
                ParseMode.Html,
                cancellationToken: cancellationToken);
        }

        return HandleAsyncInternal(callbackContext);
    }

    private async Task HandleAsyncInternal(CallbackContext callbackContext)
    {
        var queue = await _queueService.GetQueueAsync(callbackContext.CallbackData.QueueData!.QueueId, includeMembers: false, CancellationToken.None);
        if (queue == null)
        {
            await TelegramBotClient.EditMessageTextAsync(
                callbackContext.Chat.Id,
                callbackContext.MessageId,
                LocalizationProvider.GetMessage(CallbackMessageKeys.Callback_QueueHasBeenDeleted_Message, MessageParameters.None),
                replyMarkup: GetReturnToChatButton(callbackContext.CallbackData));

            return;
        }

        await HandleCallbackWithExistingQueueAsync(queue, callbackContext);
    }

    private async Task HandleCallbackWithExistingQueueAsync(Queue queue, CallbackContext callbackContext)
    {
        if (callbackContext.CallbackData.HasUserAgreement)
        {
            await HandleCallbackWithUserAgreementAsync(queue, callbackContext);
            return;
        }

        var replyMarkup = new InlineKeyboardButton[][]
        {
            new InlineKeyboardButton[] { GetDequeueButton(
                LocalizationProvider.GetMessage(CallbackMessageKeys.DequeueMeCallbackHandler.Callback_DequeueMe_AgreeToDequeue_Button, MessageParameters.None),
                callbackContext.CallbackData,
                isAgreed: true) },
            new InlineKeyboardButton[] { GetReturnToQueueButton(callbackContext.CallbackData) }
        };

        await TelegramBotClient.EditMessageTextAsync(
            callbackContext.Chat.Id,
            callbackContext.MessageId,
            LocalizationProvider.GetMessage(CallbackMessageKeys.DequeueMeCallbackHandler.Callback_DequeueMe_AreYouSureToBeDequeued_Message, new MessageParameters(queue.Name)),
            ParseMode.Html,
            replyMarkup: replyMarkup);
    }

    private async Task HandleCallbackWithUserAgreementAsync(Queue queue, CallbackContext callbackContext)
    {
        var user = await _userService.GetOrStoreUserAsync(callbackContext.Sender, CancellationToken.None);
        var responseMessage = await _queueService.TryDequeueUserAsync(user, queue.Id, CancellationToken.None)
            ? LocalizationProvider.GetMessage(CallbackMessageKeys.DequeueMeCallbackHandler.Callback_DequeueMe_Success_Message, new MessageParameters(queue.Name))
            : LocalizationProvider.GetMessage(CallbackMessageKeys.DequeueMeCallbackHandler.Callback_DequeueMe_UserDoesNotParticipate_Message, new MessageParameters(queue.Name));

        await TelegramBotClient.EditMessageTextAsync(
            callbackContext.Chat.Id,
            callbackContext.MessageId,
            responseMessage,
            ParseMode.Html,
            replyMarkup: GetReturnToQueueButton(callbackContext.CallbackData));
    }

    private InlineKeyboardButton GetDequeueButton(string buttonText, CallbackData callbackData, bool? isAgreed = null)
    {
        var buttonCallbackData = new CallbackData()
        {
            Command = CallbackCommands.DequeueMeCommand,
            TargetChatId = callbackData.TargetChatId,
            UserAgreement = isAgreed,
            QueueData = new QueueData()
            {
                QueueId = callbackData.QueueData!.QueueId,
            }
        };

        var serializedCallbackData = DataSerializer.Serialize(buttonCallbackData);
        return InlineKeyboardButton.WithCallbackData(buttonText, serializedCallbackData);
    }
}
