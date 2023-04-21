using System.Threading;
using System.Threading.Tasks;
using Enqueuer.Callbacks.CallbackHandlers.BaseClasses;
using Enqueuer.Core.Constants;
using Enqueuer.Core.TextProviders;
using Enqueuer.Services;
using Enqueuer.Services.Exceptions;
using Enqueuer.Telegram.Core.Serialization;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Enqueuer.Callbacks.CallbackHandlers;

public class EnqueueAtCallbackHandler : CallbackHandlerBaseWithReturnToQueueButton
{
    private readonly IQueueService _queueService;
    private readonly ILogger<EnqueueAtCallbackHandler> _logger;

    public EnqueueAtCallbackHandler(
        ITelegramBotClient telegramBotClient, ICallbackDataSerializer dataSerializer,
        IMessageProvider messageProvider, IQueueService queueService, ILogger<EnqueueAtCallbackHandler> logger)
        : base(telegramBotClient, dataSerializer, messageProvider)
    {
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
                MessageProvider.GetMessage(CallbackMessageKeys.OutdatedCallback_Message),
                ParseMode.Html,
                cancellationToken: cancellationToken);
        }

        return HandleAsyncInternal(callback, cancellationToken);
    }

    private async Task HandleAsyncInternal(Callback callback, CancellationToken cancellationToken)
    {
        var queueId = callback.CallbackData!.QueueData!.QueueId;
        try
        {
            (var queue, var position) = callback.CallbackData.QueueData.Position.HasValue
                ? await _queueService.EnqueueOnPositionAsync(callback.From.Id, queueId, callback.CallbackData.QueueData.Position.Value, cancellationToken)
                : await _queueService.EnqueueOnFirstAvailablePositionAsync(callback.From.Id, queueId, cancellationToken);

            await TelegramBotClient.EditMessageTextAsync(
                callback.Message.Chat,
                callback.Message.MessageId,
                MessageProvider.GetMessage(CallbackMessageKeys.EnqueueAtCallbackHandler.Callback_EnqueueAt_Success_Message, position, queue.Name),
                ParseMode.Html,
                replyMarkup: GetReturnToQueueButton(callback.CallbackData),
                cancellationToken: cancellationToken);
        }
        catch (QueueDoesNotExistException)
        {
            await TelegramBotClient.EditMessageTextAsync(
                callback.Message.Chat,
                callback.Message.MessageId,
                MessageProvider.GetMessage(CallbackMessageKeys.QueueHasBeenDeleted_Message),
                replyMarkup: GetReturnToQueueButton(callback.CallbackData),
                cancellationToken: cancellationToken);
        }
        catch (UserAlreadyParticipatesException ex)
        {
            await TelegramBotClient.EditMessageTextAsync(
                callback.Message.Chat,
                callback.Message.MessageId,
                MessageProvider.GetMessage(CallbackMessageKeys.EnqueueAtCallbackHandler.Callback_EnqueueAt_UserAlreadyParticipates_Message, ex.QueueName),
                ParseMode.Html,
                replyMarkup: GetReturnToQueueButton(callback.CallbackData),
                cancellationToken: cancellationToken);
        }
        catch (QueueIsFullException ex)
        {
            await TelegramBotClient.EditMessageTextAsync(
                callback.Message.Chat,
                callback.Message.MessageId,
                MessageProvider.GetMessage(CallbackMessageKeys.EnqueueAtCallbackHandler.Callback_EnqueueAt_QueueIsFull_Message, ex.QueueName),
                ParseMode.Html,
                replyMarkup: GetReturnToQueueButton(callback.CallbackData),
                cancellationToken: cancellationToken);
        }
        catch (PositionIsReservedException ex)
        {
            await TelegramBotClient.EditMessageTextAsync(
                callback.Message.Chat,
                callback.Message.MessageId,
                MessageProvider.GetMessage(CallbackMessageKeys.EnqueueAtCallbackHandler.Callback_EnqueueAt_PositionIsReserved_Message, ex.QueueName, ex.Position),
                ParseMode.Html,
                replyMarkup: GetQueueRelatedButton(MessageProvider.GetMessage(CallbackMessageKeys.EnqueueAtCallbackHandler.Callback_EnqueueAt_PositionIsReserved_ChooseAnother_Button), CallbackConstants.EnqueueCommand, callback.CallbackData, queueId),
                cancellationToken: cancellationToken);
        }
        catch (QueueIsDynamicException ex)
        {
            await TelegramBotClient.EditMessageTextAsync(
                callback.Message.Chat,
                callback.Message.MessageId,
                MessageProvider.GetMessage(CallbackMessageKeys.EnqueueAtCallbackHandler.Callback_EnqueueAt_QueueIsDynamicButPositionIsSpecified_Message, ex.QueueName),
                ParseMode.Html,
                replyMarkup: new InlineKeyboardMarkup(new InlineKeyboardButton[] 
                {
                    GetQueueRelatedButton(MessageProvider.GetMessage(CallbackMessageKeys.EnqueueCallbackHandler.Callback_Enqueue_FirstAvailable_Button), CallbackConstants.EnqueueAtCommand, callback.CallbackData, queueId),
                    GetReturnToQueueButton(callback.CallbackData)
                }),
                cancellationToken: cancellationToken);
        }
    }
}
