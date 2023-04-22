using System.Threading;
using System.Threading.Tasks;
using Enqueuer.Callbacks.CallbackHandlers.BaseClasses;
using Enqueuer.Core.TextProviders;
using Enqueuer.Persistence.Models;
using Enqueuer.Services;
using Enqueuer.Telegram.Core.Localization;
using Enqueuer.Telegram.Core.Serialization;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace Enqueuer.Callbacks.CallbackHandlers;

public class SwitchQueueCallbackHandler : CallbackHandlerBaseWithReturnToQueueButton
{
    private readonly IUserService _userService;
    private readonly IQueueService _queueService;

    public SwitchQueueCallbackHandler(ITelegramBotClient telegramBotClient, ICallbackDataSerializer dataSerializer, ILocalizationProvider localizationProvider, IUserService userService, IQueueService queueService)
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
        var queue = await _queueService.GetQueueAsync(callback.CallbackData!.QueueData.QueueId, includeMembers: false, cancellationToken);
        if (queue == null)
        {
            await TelegramBotClient.EditMessageTextAsync(
                callback.Message.Chat,
                callback.Message.MessageId,
                LocalizationProvider.GetMessage(CallbackMessageKeys.Callback_QueueHasBeenDeleted_Message, MessageParameters.None),
                replyMarkup: GetReturnToChatButton(callback.CallbackData),
                cancellationToken: cancellationToken);

            return;
        }

        await HandleCallbackWithExistingQueueAsync(queue, callback, cancellationToken);
    }

    private async Task HandleCallbackWithExistingQueueAsync(Queue queue, Callback callback, CancellationToken cancellationToken)
    {
        var user = await _userService.GetOrStoreUserAsync(callback.From, cancellationToken);
        var isDynamic = queue.IsDynamic;
        await _queueService.SwitchQueueStatusAsync(queue.Id, cancellationToken);

        if (isDynamic)
        {
            await TelegramBotClient.EditMessageTextAsync(
                callback.Message.Chat,
                callback.Message.MessageId,
                LocalizationProvider.GetMessage(CallbackMessageKeys.SwitchQueueCallbackHandler.Callback_SwitchQueue_QueueIsNotDynamicNow_Message, new MessageParameters(queue.Name)),
                ParseMode.Html,
                replyMarkup: GetReturnToQueueButton(callback.CallbackData),
                cancellationToken: cancellationToken);

            return;
        }

        await TelegramBotClient.SendTextMessageAsync(
            queue.GroupId,
            LocalizationProvider.GetMessage(CallbackMessageKeys.SwitchQueueCallbackHandler.Callback_SwitchQueue_QueueIsDynamicNow_PublicChat_Message, new MessageParameters(user.FullName, queue.Name)),
            ParseMode.Html,
            cancellationToken: cancellationToken);

        await TelegramBotClient.EditMessageTextAsync(
            callback.Message.Chat,
            callback.Message.MessageId,
            LocalizationProvider.GetMessage(CallbackMessageKeys.SwitchQueueCallbackHandler.Callback_SwitchQueue_QueueIsDynamicNow_Message, new MessageParameters(queue.Name)),
            ParseMode.Html,
            replyMarkup: GetReturnToQueueButton(callback.CallbackData),
            cancellationToken: cancellationToken);
    }
}
