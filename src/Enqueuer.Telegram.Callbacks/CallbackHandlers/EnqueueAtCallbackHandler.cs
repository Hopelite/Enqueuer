using System.Threading;
using System.Threading.Tasks;
using Enqueuer.Messaging.Core.Constants;
using Enqueuer.Messaging.Core.Localization;
using Enqueuer.Messaging.Core.Serialization;
using Enqueuer.Messaging.Core.Types.Callbacks;
using Enqueuer.Services;
using Enqueuer.Services.Exceptions;
using Enqueuer.Telegram.Callbacks.Helpers.Markup;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace Enqueuer.Telegram.Callbacks.CallbackHandlers;

public class EnqueueAtCallbackHandler : CallbackHandlerBase
{
    private readonly ICallbackDataSerializer _dataSerializer;
    private readonly IQueueService _queueService;

    public EnqueueAtCallbackHandler(ITelegramBotClient telegramBotClient, ICallbackDataSerializer dataSerializer, ILocalizationProvider localizationProvider, IQueueService queueService)
        : base(telegramBotClient, localizationProvider)
    {
        _dataSerializer = dataSerializer;
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
        var queueId = callbackContext.CallbackData!.QueueData!.QueueId;
        try
        {
            (var queue, var position) = callbackContext.CallbackData.QueueData.Position.HasValue
                ? await _queueService.EnqueueOnPositionAsync(callbackContext.Sender.Id, queueId, callbackContext.CallbackData.QueueData.Position.Value, cancellationToken)
                : await _queueService.EnqueueOnFirstAvailablePositionAsync(callbackContext.Sender.Id, queueId, cancellationToken);

            var replyMarkup = ReplyMarkupBuilder.Create(_dataSerializer, LocalizationProvider)
                .WithReturnToQueueButton(callbackContext.CallbackData, LocalizationProvider.GetMessage(CallbackMessageKeys.Callback_Return_Button, MessageParameters.None))
                .Build();

            await TelegramBotClient.EditMessageTextAsync(
                callbackContext.Chat.Id,
                callbackContext.MessageId,
                LocalizationProvider.GetMessage(CallbackMessageKeys.EnqueueAtCallbackHandler.Callback_EnqueueAt_Success_Message, new MessageParameters(position.ToString(), queue.Name)),
                ParseMode.Html,
                replyMarkup: replyMarkup,
                cancellationToken: cancellationToken);
        }
        catch (UserDoesNotExistException)
        {
            // TODO: handle this case
            throw;
        }
        catch (QueueDoesNotExistException)
        {
            var replyMarkup = ReplyMarkupBuilder.Create(_dataSerializer, LocalizationProvider)
                .WithReturnToQueueButton(callbackContext.CallbackData, LocalizationProvider.GetMessage(CallbackMessageKeys.Callback_Return_Button, MessageParameters.None))
                .Build();

            await TelegramBotClient.EditMessageTextAsync(
                callbackContext.Chat.Id,
                callbackContext.MessageId,
                LocalizationProvider.GetMessage(CallbackMessageKeys.Callback_QueueHasBeenDeleted_Message, MessageParameters.None),
                replyMarkup: replyMarkup,
                cancellationToken: cancellationToken);
        }
        catch (UserAlreadyParticipatesException ex)
        {
            var replyMarkup = ReplyMarkupBuilder.Create(_dataSerializer, LocalizationProvider)
                .WithReturnToQueueButton(callbackContext.CallbackData, LocalizationProvider.GetMessage(CallbackMessageKeys.Callback_Return_Button, MessageParameters.None))
                .Build();

            await TelegramBotClient.EditMessageTextAsync(
                callbackContext.Chat.Id,
                callbackContext.MessageId,
                LocalizationProvider.GetMessage(CallbackMessageKeys.EnqueueAtCallbackHandler.Callback_EnqueueAt_UserAlreadyParticipates_Message, new MessageParameters(ex.QueueName)),
                ParseMode.Html,
                replyMarkup: replyMarkup,
                cancellationToken: cancellationToken);
        }
        catch (QueueIsFullException ex)
        {
            var replyMarkup = ReplyMarkupBuilder.Create(_dataSerializer, LocalizationProvider)
                .WithReturnToQueueButton(callbackContext.CallbackData, LocalizationProvider.GetMessage(CallbackMessageKeys.Callback_Return_Button, MessageParameters.None))
                .Build();

            await TelegramBotClient.EditMessageTextAsync(
                callbackContext.Chat.Id,
                callbackContext.MessageId,
                LocalizationProvider.GetMessage(CallbackMessageKeys.EnqueueAtCallbackHandler.Callback_EnqueueAt_QueueIsFull_Message, new MessageParameters(ex.QueueName)),
                ParseMode.Html,
                replyMarkup: replyMarkup,
                cancellationToken: cancellationToken);
        }
        catch (PositionIsReservedException ex)
        {
            var replyMarkup = ReplyMarkupBuilder.Create(_dataSerializer, LocalizationProvider)
                .WithQueueRelatedButton(
                    LocalizationProvider.GetMessage(CallbackMessageKeys.EnqueueAtCallbackHandler.Callback_EnqueueAt_PositionIsReserved_ChooseAnother_Button, MessageParameters.None),
                    CallbackCommands.EnqueueCommand,
                    callbackContext.CallbackData,
                    queueId)
                .Build();

            await TelegramBotClient.EditMessageTextAsync(
                callbackContext.Chat.Id,
                callbackContext.MessageId,
                LocalizationProvider.GetMessage(CallbackMessageKeys.EnqueueAtCallbackHandler.Callback_EnqueueAt_PositionIsReserved_Message, new MessageParameters(ex.QueueName, ex.Position.ToString())),
                ParseMode.Html,
                replyMarkup: replyMarkup,
                cancellationToken: cancellationToken);
        }
        catch (QueueIsDynamicException ex)
        {
            var replyMarkup = ReplyMarkupBuilder.Create(_dataSerializer, LocalizationProvider)
                .WithQueueRelatedButton(
                    LocalizationProvider.GetMessage(CallbackMessageKeys.EnqueueCallbackHandler.Callback_Enqueue_FirstAvailable_Button, MessageParameters.None),
                    CallbackCommands.EnqueueAtCommand,
                    callbackContext.CallbackData,
                    queueId)
                .WithReturnToQueueButton(callbackContext.CallbackData, LocalizationProvider.GetMessage(CallbackMessageKeys.Callback_Return_Button, MessageParameters.None))
                .Build();

            await TelegramBotClient.EditMessageTextAsync(
                callbackContext.Chat.Id,
                callbackContext.MessageId,
                LocalizationProvider.GetMessage(CallbackMessageKeys.EnqueueAtCallbackHandler.Callback_EnqueueAt_QueueIsDynamicButPositionIsSpecified_Message, new MessageParameters(ex.QueueName)),
                ParseMode.Html,
                replyMarkup: replyMarkup,
                cancellationToken: cancellationToken);
        }
    }
}
