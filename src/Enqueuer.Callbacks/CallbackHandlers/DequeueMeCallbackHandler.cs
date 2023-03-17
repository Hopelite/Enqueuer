using System.Threading;
using System.Threading.Tasks;
using Enqueuer.Callbacks.CallbackHandlers.BaseClasses;
using Enqueuer.Data.DataSerialization;
using Enqueuer.Data.TextProviders;
using Enqueuer.Persistence.Models;
using Enqueuer.Services;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

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
                MessageProvider.GetMessage(CallbackMessageKeys.EnqueueAtCallbackHandler.EnqueueAtCallback_QueueHasBeenDeleted_Message),
                replyMarkup: GetReturnToChatButton(callback.CallbackData));

            return;
        }

        await HandleCallbackWithExistingQueueAsync(queue, callback);
    }

    private async Task HandleCallbackWithExistingQueueAsync(Queue queue, Callback callback)
    {
        string responseMessage = $"You've already dequeued from the '<b>{queue.Name}</b>' queue.";
        var user = await _userService.GetOrStoreUserAsync(callback.From, CancellationToken.None);
        if (await _queueService.TryDequeueUserAsync(user, queue.Id, CancellationToken.None))
        {
            responseMessage = $"Successfully removed from the '<b>{queue.Name}</b>' queue!";
        }

        await TelegramBotClient.EditMessageTextAsync(
            callback.Message.Chat,
            callback.Message.MessageId,
            responseMessage,
            ParseMode.Html,
            replyMarkup: GetReturnToQueueButton(callback.CallbackData!));
    }
}
