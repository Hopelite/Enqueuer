using System.Threading;
using System.Threading.Tasks;
using Enqueuer.Messaging.Core.Localization;
using Enqueuer.Messaging.Core.Serialization;
using Enqueuer.Messaging.Core.Types.Callbacks;
using Enqueuer.Persistence.Models;
using Enqueuer.Services;
using Enqueuer.Telegram.Callbacks.Helpers.Markup;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace Enqueuer.Telegram.Callbacks.CallbackHandlers;

public class SwitchQueueCallbackHandler : CallbackHandlerBase
{
    private readonly ICallbackDataSerializer _dataSerializer;
    private readonly IUserService _userService;
    private readonly IQueueService _queueService;

    public SwitchQueueCallbackHandler(ITelegramBotClient telegramBotClient, ICallbackDataSerializer dataSerializer, ILocalizationProvider localizationProvider, IUserService userService, IQueueService queueService)
        : base(telegramBotClient, localizationProvider)
    {
        _dataSerializer = dataSerializer;
        _userService = userService;
        _queueService = queueService;
    }

    protected override Task HandleAsyncImplementation(CallbackContext callbackContext, CancellationToken cancellationToken)
    {
        if (callbackContext.CallbackData.QueueData == null)
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
        var queue = await _queueService.GetQueueAsync(callbackContext.CallbackData.QueueData!.QueueId, includeMembers: false, cancellationToken);
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

        await HandleCallbackWithExistingQueueAsync(queue, callbackContext, cancellationToken);
    }

    private async Task HandleCallbackWithExistingQueueAsync(Queue queue, CallbackContext callbackContext, CancellationToken cancellationToken)
    {
        var user = await _userService.GetOrStoreUserAsync(callbackContext.Sender, cancellationToken);
        var isDynamic = queue.IsDynamic;
        await _queueService.SwitchQueueStatusAsync(queue.Id, cancellationToken);

        var replyButton = ReplyMarkupBuilder.Create(_dataSerializer, LocalizationProvider)
            .WithReturnToQueueButton(callbackContext.CallbackData)
            .Build();

        if (isDynamic)
        {
            await TelegramBotClient.EditMessageTextAsync(
                callbackContext.Chat.Id,
                callbackContext.MessageId,
                LocalizationProvider.GetMessage(CallbackMessageKeys.SwitchQueueCallbackHandler.Callback_SwitchQueue_QueueIsNotDynamicNow_Message, new MessageParameters(queue.Name)),
                ParseMode.Html,
                replyMarkup: replyButton,
                cancellationToken: cancellationToken);

            return;
        }

        await TelegramBotClient.SendTextMessageAsync(
            queue.GroupId,
            LocalizationProvider.GetMessage(CallbackMessageKeys.SwitchQueueCallbackHandler.Callback_SwitchQueue_QueueIsDynamicNow_PublicChat_Message, new MessageParameters(user.FullName, queue.Name)),
            ParseMode.Html,
            cancellationToken: cancellationToken);

        await TelegramBotClient.EditMessageTextAsync(
            callbackContext.Chat.Id,
            callbackContext.MessageId,
            LocalizationProvider.GetMessage(CallbackMessageKeys.SwitchQueueCallbackHandler.Callback_SwitchQueue_QueueIsDynamicNow_Message, new MessageParameters(queue.Name)),
            ParseMode.Html,
            replyMarkup: replyButton,
            cancellationToken: cancellationToken);
    }
}
