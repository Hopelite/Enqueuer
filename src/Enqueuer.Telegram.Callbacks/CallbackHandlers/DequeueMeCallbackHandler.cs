using System.Threading;
using System.Threading.Tasks;
using Enqueuer.Messaging.Core.Localization;
using Enqueuer.Messaging.Core.Serialization;
using Enqueuer.Messaging.Core.Types.Callbacks;
using Enqueuer.Persistence.Models;
using Enqueuer.Services;
using Enqueuer.Telegram.Callbacks.Helpers.Markup;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace Enqueuer.Telegram.Callbacks.CallbackHandlers;

public class DequeueMeCallbackHandler : CallbackHandlerBase
{
    private readonly ICallbackDataSerializer _dataSerializer;
    private readonly IUserService _userService;
    private readonly IQueueService _queueService;
    private readonly ILogger<DequeueMeCallbackHandler> _logger;

    public DequeueMeCallbackHandler(ITelegramBotClient telegramBotClient, ICallbackDataSerializer dataSerializer, ILocalizationProvider localizationProvider, IUserService userService, IQueueService queueService, ILogger<DequeueMeCallbackHandler> logger)
        : base(telegramBotClient, localizationProvider)
    {
        _dataSerializer = dataSerializer;
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
            var replyMarkup = new ReturnToChatMarkup(_dataSerializer, LocalizationProvider)
                .Create(callbackContext.CallbackData);

            await TelegramBotClient.EditMessageTextAsync(
                callbackContext.Chat.Id,
                callbackContext.MessageId,
                LocalizationProvider.GetMessage(CallbackMessageKeys.Callback_QueueHasBeenDeleted_Message, MessageParameters.None),
                replyMarkup: replyMarkup);

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

        var replyMarkup = ReplyMarkupBuilder.Create(_dataSerializer, LocalizationProvider)
            .WithDequeueButton(
                LocalizationProvider.GetMessage(CallbackMessageKeys.DequeueMeCallbackHandler.Callback_DequeueMe_AgreeToDequeue_Button, MessageParameters.None),
                callbackContext.CallbackData,
                isAgreed: true)
            .FromNewRow()
            .WithReturnToQueueButton(callbackContext.CallbackData, LocalizationProvider.GetMessage(CallbackMessageKeys.Callback_Return_Button, MessageParameters.None))
            .Build();

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

        var replyMarkup = ReplyMarkupBuilder.Create(_dataSerializer, LocalizationProvider)
            .WithReturnToQueueButton(callbackContext.CallbackData, LocalizationProvider.GetMessage(CallbackMessageKeys.Callback_Return_Button, MessageParameters.None))
            .Build();

        await TelegramBotClient.EditMessageTextAsync(
            callbackContext.Chat.Id,
            callbackContext.MessageId,
            responseMessage,
            ParseMode.Html,
            replyMarkup: replyMarkup);
    }
}
