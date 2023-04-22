using System.Threading;
using System.Threading.Tasks;
using Enqueuer.Callbacks.CallbackHandlers.BaseClasses;
using Enqueuer.Core.Constants;
using Enqueuer.Core.TextProviders;
using Enqueuer.Persistence.Models;
using Enqueuer.Services;
using Enqueuer.Telegram.Core;
using Enqueuer.Telegram.Core.Localization;
using Enqueuer.Telegram.Core.Serialization;
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

    public DequeueMeCallbackHandler(ITelegramBotClient telegramBotClient, ICallbackDataSerializer dataSerializer, ILocalizationProvider localizationProvider, IUserService userService, IQueueService queueService, ILogger<DequeueMeCallbackHandler> logger)
        : base(telegramBotClient, dataSerializer, localizationProvider)
    {
        _userService = userService;
        _queueService = queueService;
        _logger = logger;
    }

    protected override Task HandleAsyncImplementation(Callback callback, CancellationToken cancellationToken)
    {
        if (callback.CallbackData?.QueueData == null)
        {
            _logger.LogWarning("Handled outdated callback.");
            return TelegramBotClient.EditMessageTextAsync(
                callback.Message.Chat,
                callback.Message.MessageId,
                LocalizationProvider.GetMessage(CallbackMessageKeys.Callback_OutdatedCallback_Message, MessageParameters.None),
                ParseMode.Html,
                cancellationToken: cancellationToken);
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
                LocalizationProvider.GetMessage(CallbackMessageKeys.Callback_QueueHasBeenDeleted_Message, MessageParameters.None),
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
            new InlineKeyboardButton[] { GetDequeueButton(LocalizationProvider.GetMessage(CallbackMessageKeys.DequeueMeCallbackHandler.Callback_DequeueMe_AgreeToDequeue_Button, MessageParameters.None), callback.CallbackData, true) },
            new InlineKeyboardButton[] { GetReturnToQueueButton(callback.CallbackData) }
        };

        await TelegramBotClient.EditMessageTextAsync(
            callback.Message.Chat,
            callback.Message.MessageId,
            LocalizationProvider.GetMessage(CallbackMessageKeys.DequeueMeCallbackHandler.Callback_DequeueMe_AreYouSureToBeDequeued_Message, new MessageParameters(queue.Name)),
            ParseMode.Html,
            replyMarkup: replyMarkup);
    }

    private async Task HandleCallbackWithUserAgreementAsync(Queue queue, Callback callback)
    {
        var user = await _userService.GetOrStoreUserAsync(callback.From, CancellationToken.None);
        var responseMessage = await _queueService.TryDequeueUserAsync(user, queue.Id, CancellationToken.None)
            ? LocalizationProvider.GetMessage(CallbackMessageKeys.DequeueMeCallbackHandler.Callback_DequeueMe_Success_Message, new MessageParameters(queue.Name))
            : LocalizationProvider.GetMessage(CallbackMessageKeys.DequeueMeCallbackHandler.Callback_DequeueMe_UserDoesNotParticipate_Message, new MessageParameters(queue.Name));

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
