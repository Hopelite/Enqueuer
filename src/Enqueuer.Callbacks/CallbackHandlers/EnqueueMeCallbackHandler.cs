using System.Threading;
using System.Threading.Tasks;
using Enqueuer.Data.TextProviders;
using Enqueuer.Services;
using Enqueuer.Services.Exceptions;
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

    public EnqueueMeCallbackHandler(
        ITelegramBotClient telegramBotClient, IQueueService queueService,
        IGroupService groupService, IMessageProvider messageProvider, ILogger<EnqueueMeCallbackHandler> logger)
    {
        _telegramBotClient = telegramBotClient;
        _queueService = queueService;
        _groupService = groupService;
        _messageProvider = messageProvider;
        _logger = logger;
    }

    public Task HandleAsync(Callback callback, CancellationToken cancellationToken)
    {
        if (callback.CallbackData?.QueueData == null)
        {
            _logger.LogWarning("Handled outdated callback.");
            return _telegramBotClient.EditMessageTextAsync(
                callback.Message.Chat,
                callback.Message.MessageId,
                _messageProvider.GetMessage(CallbackMessageKeys.OutdatedCallback_Message),
                ParseMode.Html,
                cancellationToken: cancellationToken);
        }

        return HandleAsyncInternal(callback, cancellationToken);
    }

    private async Task HandleAsyncInternal(Callback callback, CancellationToken cancellationToken)
    {
        await _groupService.AddOrUpdateUserAndGroupAsync(callback.Message!.Chat, callback.From, includeQueues: false, CancellationToken.None);
        var queueId = callback.CallbackData!.QueueData!.QueueId;
        try
        {
            var response = await _queueService.EnqueueOnFirstAvailablePositionAsync(callback.From.Id, queueId, cancellationToken);
            await _telegramBotClient.AnswerCallbackQueryAsync(
                callback.Id,
                _messageProvider.GetMessage(CallbackMessageKeys.EnqueueMeCallbackHandler.EnqueueMeCallback_SuccessfullyEnqueued_Notification, response.Queue.Name, response.Position),
                cancellationToken: cancellationToken);
        }
        catch (QueueDoesNotExistException)
        {
            await _telegramBotClient.EditMessageTextAsync(
                callback.Message.Chat,
                callback.Message.MessageId,
                _messageProvider.GetMessage(CallbackMessageKeys.EnqueueMeCallbackHandler.EnqueueMeCallback_QueueHasBeenDeleted_Message),
                ParseMode.Html,
                cancellationToken: cancellationToken);

            await _telegramBotClient.AnswerCallbackQueryAsync(callback.Id, cancellationToken: cancellationToken);
        }
        catch (UserAlreadyParticipatesException ex)
        {
            await _telegramBotClient.AnswerCallbackQueryAsync(
                callback.Id,
                _messageProvider.GetMessage(CallbackMessageKeys.EnqueueMeCallbackHandler.EnqueueMeCallback_UserAlreadyParticipates_Notification, ex.QueueName),
                cancellationToken: cancellationToken);
        }
        catch (QueueIsFullException ex)
        {
            await _telegramBotClient.AnswerCallbackQueryAsync(
                callback.Id,
                _messageProvider.GetMessage(CallbackMessageKeys.EnqueueAtCallbackHandler.EnqueueAtCallback_QueueIsFull_Message, ex.QueueName),
                cancellationToken: cancellationToken);
        }
    }
}
