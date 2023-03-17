using System.Threading;
using System.Threading.Tasks;
using Enqueuer.Callbacks.CallbackHandlers.BaseClasses;
using Enqueuer.Data.DataSerialization;
using Enqueuer.Data.TextProviders;
using Enqueuer.Persistence.Models;
using Enqueuer.Services;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Enqueuer.Callbacks.CallbackHandlers;

public class SwitchQueueDynamicHandler : CallbackHandlerBaseWithReturnToQueueButton
{
    private readonly IUserService _userService;
    private readonly IQueueService _queueService;
    private readonly ILogger<SwitchQueueDynamicHandler> _logger;

    public SwitchQueueDynamicHandler(ITelegramBotClient telegramBotClient, IDataSerializer dataSerializer, IMessageProvider messageProvider, IUserService userService, IQueueService queueService, ILogger<SwitchQueueDynamicHandler> logger)
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
                "This queue has been deleted.",
                replyMarkup: GetReturnToChatButton(callback.CallbackData));

            return;
        }

        await HandleCallbackWithExistingQueueAsync(queue, callback);
    }

    private async Task HandleCallbackWithExistingQueueAsync(Queue queue, Callback callback)
    {
        await Task.CompletedTask;
        // if (queue.IsDynamic)
        // {
        //     queue.IsDynamic = false;
        //     await _queueService.UpdateQueueAsync(queue);
        //     return await botClient.EditMessageTextAsync(
        //         callbackQuery.Message.Chat,
        //         callbackQuery.Message.MessageId,
        //         $"Queue <b>'{queue.Name}' is not dynamic now.</b>",
        //         ParseMode.Html,
        //         replyMarkup: GetReturnToQueueButton(callback));
        // }

        // queue.IsDynamic = true;
        // await _queueService.UpdateQueueAsync(queue);
        // await _userInQueueService.CompressQueuePositionsAsync(queue);

        // var chat = queue.Group;
        // await botClient.SendTextMessageAsync(
        //     chat.Id,
        //     $"{callbackQuery.From.FirstName} {callbackQuery.From.LastName + ' ' ?? string.Empty}made <b>'{queue.Name}'</b> queue dynamic. Keep up!",
        //     ParseMode.Html);

        // return await botClient.EditMessageTextAsync(
        //     callbackQuery.Message.Chat,
        //     callbackQuery.Message.MessageId,
        //     $"Queue <b>'{queue.Name}' is dynamic now.</b>",
        //     ParseMode.Html,
        //     replyMarkup: GetReturnToQueueButton(callback));
    }
}
