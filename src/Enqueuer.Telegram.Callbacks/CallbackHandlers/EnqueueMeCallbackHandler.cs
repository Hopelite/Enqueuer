using System.Threading;
using System.Threading.Tasks;
using Enqueuer.Services;
using Enqueuer.Services.Exceptions;
using Enqueuer.Messaging.Core.Localization;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Enqueuer.Messaging.Core.Types.Callbacks;

namespace Enqueuer.Telegram.Callbacks.CallbackHandlers;

public class EnqueueMeCallbackHandler : ICallbackHandler
{
    private readonly ITelegramBotClient _telegramBotClient;
    private readonly IQueueService _queueService;
    private readonly IGroupService _groupService;
    private readonly ILocalizationProvider _localizationProvider;

    public EnqueueMeCallbackHandler(ITelegramBotClient telegramBotClient, IQueueService queueService, IGroupService groupService, ILocalizationProvider localizationProvider)
    {
        _telegramBotClient = telegramBotClient;
        _queueService = queueService;
        _groupService = groupService;
        _localizationProvider = localizationProvider;
    }

    public Task HandleAsync(CallbackContext callbackContext, CancellationToken cancellationToken)
    {
        if (callbackContext.CallbackData.QueueData == null)
        {
            return _telegramBotClient.EditMessageTextAsync(
                callbackContext.Chat.Id,
                callbackContext.MessageId,
                _localizationProvider.GetMessage(CallbackMessageKeys.Callback_OutdatedCallback_Message, MessageParameters.None),
                ParseMode.Html,
                cancellationToken: cancellationToken);
        }

        return HandleAsyncInternal(callbackContext, cancellationToken);
    }

    private async Task HandleAsyncInternal(CallbackContext callbackContext, CancellationToken cancellationToken)
    {
        await _groupService.AddOrUpdateUserAndGroupAsync(callbackContext.Chat, callbackContext.Sender, includeQueues: false, cancellationToken);
        var queueId = callbackContext.CallbackData!.QueueData!.QueueId;
        try
        {
            var response = await _queueService.EnqueueOnFirstAvailablePositionAsync(callbackContext.Sender.Id, queueId, cancellationToken);
            await _telegramBotClient.AnswerCallbackQueryAsync(
                callbackContext.QueryId,
                _localizationProvider.GetMessage(CallbackMessageKeys.EnqueueMeCallbackHandler.Callback_EnqueueMe_SuccessfullyEnqueued_Alert, new MessageParameters(response.Queue.Name, response.Position.ToString())),
                cancellationToken: cancellationToken);
        }
        catch (QueueDoesNotExistException)
        {
            await _telegramBotClient.EditMessageTextAsync(
                callbackContext.Chat.Id,
                callbackContext.MessageId,
                _localizationProvider.GetMessage(CallbackMessageKeys.EnqueueMeCallbackHandler.Callback_EnqueueMe_QueueHasBeenDeleted_Message, MessageParameters.None),
                ParseMode.Html,
                cancellationToken: cancellationToken);

            await _telegramBotClient.AnswerCallbackQueryAsync(callbackContext.QueryId, cancellationToken: cancellationToken);
        }
        catch (UserAlreadyParticipatesException ex)
        {
            await _telegramBotClient.AnswerCallbackQueryAsync(
                callbackContext.QueryId,
                _localizationProvider.GetMessage(CallbackMessageKeys.EnqueueMeCallbackHandler.Callback_EnqueueMe_UserAlreadyParticipates_Alert, new MessageParameters(ex.QueueName)),
                cancellationToken: cancellationToken);
        }
        catch (QueueIsFullException ex)
        {
            await _telegramBotClient.AnswerCallbackQueryAsync(
                callbackContext.QueryId,
                _localizationProvider.GetMessage(CallbackMessageKeys.EnqueueAtCallbackHandler.Callback_EnqueueAt_QueueIsFull_Message, new MessageParameters(ex.QueueName)),
                cancellationToken: cancellationToken);
        }
    }
}
