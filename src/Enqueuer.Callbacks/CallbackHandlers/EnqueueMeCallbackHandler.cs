using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Enqueuer.Data.TextProviders;
using Enqueuer.Services;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace Enqueuer.Callbacks.CallbackHandlers;

public class EnqueueMeCallbackHandler : ICallbackHandler
{
    private readonly ITelegramBotClient _telegramBotClient;
    private readonly IQueueService _queueService;
    private readonly IGroupService _groupService;
    private readonly IMessageProvider _messageProvider;
    private readonly ILogger<EnqueueMeCallbackHandler> _logger;

    public EnqueueMeCallbackHandler(ITelegramBotClient telegramBotClient, IQueueService queueService, IGroupService groupService, IMessageProvider messageProvider, ILogger<EnqueueMeCallbackHandler> logger)
    {
        _telegramBotClient = telegramBotClient;
        _queueService = queueService;
        _groupService = groupService;
        _messageProvider = messageProvider;
        _logger = logger;
    }

    public Task HandleAsync(Callback callback)
    {
        if (callback.CallbackData?.QueueData == null)
        {
            _logger.LogWarning("Handled outdated callback.");
            return _telegramBotClient.EditMessageTextAsync(
                callback.Message.Chat,
                callback.Message.MessageId,
                _messageProvider.GetMessage(CallbackMessageKeys.OutdatedCallback_Message),
                ParseMode.Html);
        }

        return HandleAsyncInternal(callback);
    }

    private async Task HandleAsyncInternal(Callback callback)
    {
        (var _, var user) = await _groupService.AddOrUpdateUserAndGroupAsync(callback.Message!.Chat, callback.From, includeQueues: false, CancellationToken.None);
        var queue = await _queueService.GetQueueAsync(callback.CallbackData!.QueueData.QueueId, includeMembers: true, CancellationToken.None);
        if (queue == null)
        {
            await _telegramBotClient.EditMessageTextAsync(
                callback.Message.Chat,
                callback.Message.MessageId,
                _messageProvider.GetMessage(CallbackMessageKeys.EnqueueMeCallbackHandler.EnqueueMeCallback_QueueHasBeenDeleted_Message),
                ParseMode.Html);

            return;
        }

        if (queue.Members.Any(m => m.UserId == user.Id))
        {
            await _telegramBotClient.AnswerCallbackQueryAsync(
                callback.Id,
                _messageProvider.GetMessage(CallbackMessageKeys.EnqueueMeCallbackHandler.EnqueueMeCallback_UserAlreadyParticipates_Notification, queue.Name));
            return;
        }

        var position = await _queueService.AddAtFirstAvailablePosition(user, queue.Id, CancellationToken.None);
        await _telegramBotClient.AnswerCallbackQueryAsync(
            callback.Id,
            _messageProvider.GetMessage(CallbackMessageKeys.EnqueueMeCallbackHandler.EnqueueMeCallback_SuccessfullyEnqueued_Notification, queue.Name, position));
    }
}
