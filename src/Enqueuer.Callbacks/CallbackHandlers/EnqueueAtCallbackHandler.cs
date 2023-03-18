using System.Threading;
using System.Threading.Tasks;
using Enqueuer.Callbacks.CallbackHandlers.BaseClasses;
using Enqueuer.Data;
using Enqueuer.Data.DataSerialization;
using Enqueuer.Data.TextProviders;
using Enqueuer.Persistence.Extensions;
using Enqueuer.Persistence.Models;
using Enqueuer.Services;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using User = Enqueuer.Persistence.Models.User;

namespace Enqueuer.Callbacks.CallbackHandlers;

public class EnqueueAtCallbackHandler : CallbackHandlerBaseWithReturnToQueueButton
{
    private readonly IUserService _userService;
    private readonly IQueueService _queueService;
    private readonly ILogger<EnqueueCallbackHandler> _logger;

    public EnqueueAtCallbackHandler(ITelegramBotClient telegramBotClient, IDataSerializer dataSerializer, IMessageProvider messageProvider, IUserService userService, IQueueService queueService, ILogger<EnqueueCallbackHandler> logger)
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
        var queue = await _queueService.GetQueueAsync(callback.CallbackData!.QueueData.QueueId, includeMembers: true, CancellationToken.None);
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
        var user = await _userService.GetOrStoreUserAsync(callback.From, CancellationToken.None);
        if (user.IsParticipatingIn(queue))
        {
            await TelegramBotClient.EditMessageTextAsync(
                callback.Message.Chat,
                callback.Message.MessageId,
                MessageProvider.GetMessage(CallbackMessageKeys.EnqueueAtCallbackHandler.EnqueueAtCallback_UserAlreadyParticipates_Message, queue.Name),
                ParseMode.Html,
                replyMarkup: GetReturnToQueueButton(callback.CallbackData!));

            return;
        }

        if (queue.IsDynamic && HasSpecifiedPosition(callback.CallbackData!))
        {
            await TelegramBotClient.EditMessageTextAsync(
                callback.Message.Chat,
                callback.Message.MessageId,
                MessageProvider.GetMessage(CallbackMessageKeys.EnqueueAtCallbackHandler.EnqueueAtCallback_QueueIsDynamicButPositionIsSpecified_Message, queue.Name),
                ParseMode.Html,
                replyMarkup: GetReturnToQueueButton(callback.CallbackData));

            return;
        }

        var message = HasSpecifiedPosition(callback.CallbackData)
            ? await HandleCallbackWithSpecifiedPosition(callback.CallbackData, queue, user)
            : await HandleCallbackWithoutPositionProvided(queue, user);

        await TelegramBotClient.EditMessageTextAsync(
            callback.Message.Chat,
            callback.Message.MessageId,
            message,
            ParseMode.Html,
            replyMarkup: GetReturnToQueueButton(callback.CallbackData));
    }

    private async Task<string> HandleCallbackWithSpecifiedPosition(CallbackData callbackData, Queue queue, User user)
    {
        var position = callbackData.QueueData.Position!.Value;
        if (await _queueService.TryEnqueueUserOnPositionAsync(user, queue.Id, position, CancellationToken.None))
        {
            return MessageProvider.GetMessage(CallbackMessageKeys.EnqueueAtCallbackHandler.EnqueueAtCallback_PositionIsReserved_Message, position, queue.Name);
        }

        return MessageProvider.GetMessage(CallbackMessageKeys.EnqueueAtCallbackHandler.EnqueueAtCallback_Success_Message, queue.Name, position);
    }

    private async Task<string> HandleCallbackWithoutPositionProvided(Queue queue, User user)
    {
        var firstPositionAvailable = await _queueService.AddAtFirstAvailablePosition(user, queue.Id, CancellationToken.None);
        return MessageProvider.GetMessage(CallbackMessageKeys.EnqueueAtCallbackHandler.EnqueueAtCallback_Success_Message, queue.Name, firstPositionAvailable);
    }

    private static bool HasSpecifiedPosition(CallbackData callbackData)
    {
        return callbackData.QueueData.Position.HasValue;
    }
}
