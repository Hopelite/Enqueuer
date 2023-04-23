using System.Threading;
using System.Threading.Tasks;
using Enqueuer.Services;
using Enqueuer.Services.Exceptions;
using Enqueuer.Telegram.Callbacks.CallbackHandlers.BaseClasses;
using Enqueuer.Telegram.Core.Constants;
using Enqueuer.Telegram.Core.Localization;
using Enqueuer.Telegram.Core.Serialization;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Enqueuer.Telegram.Callbacks.CallbackHandlers;

public class EnqueueAtCallbackHandler : CallbackHandlerBaseWithReturnToQueueButton
{
    private readonly IQueueService _queueService;

    public EnqueueAtCallbackHandler(ITelegramBotClient telegramBotClient, ICallbackDataSerializer dataSerializer, ILocalizationProvider localizationProvider, IQueueService queueService)
        : base(telegramBotClient, dataSerializer, localizationProvider)
    {
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
        var queueId = callback.CallbackData!.QueueData!.QueueId;
        try
        {
            (var queue, var position) = callback.CallbackData.QueueData.Position.HasValue
                ? await _queueService.EnqueueOnPositionAsync(callback.From.Id, queueId, callback.CallbackData.QueueData.Position.Value, cancellationToken)
                : await _queueService.EnqueueOnFirstAvailablePositionAsync(callback.From.Id, queueId, cancellationToken);

            await TelegramBotClient.EditMessageTextAsync(
                callback.Message.Chat,
                callback.Message.MessageId,
                LocalizationProvider.GetMessage(CallbackMessageKeys.EnqueueAtCallbackHandler.Callback_EnqueueAt_Success_Message, new MessageParameters(position.ToString(), queue.Name)),
                ParseMode.Html,
                replyMarkup: GetReturnToQueueButton(callback.CallbackData),
                cancellationToken: cancellationToken);
        }
        catch (QueueDoesNotExistException)
        {
            await TelegramBotClient.EditMessageTextAsync(
                callback.Message.Chat,
                callback.Message.MessageId,
                LocalizationProvider.GetMessage(CallbackMessageKeys.Callback_QueueHasBeenDeleted_Message, MessageParameters.None),
                replyMarkup: GetReturnToQueueButton(callback.CallbackData),
                cancellationToken: cancellationToken);
        }
        catch (UserAlreadyParticipatesException ex)
        {
            await TelegramBotClient.EditMessageTextAsync(
                callback.Message.Chat,
                callback.Message.MessageId,
                LocalizationProvider.GetMessage(CallbackMessageKeys.EnqueueAtCallbackHandler.Callback_EnqueueAt_UserAlreadyParticipates_Message, new MessageParameters(ex.QueueName)),
                ParseMode.Html,
                replyMarkup: GetReturnToQueueButton(callback.CallbackData),
                cancellationToken: cancellationToken);
        }
        catch (QueueIsFullException ex)
        {
            await TelegramBotClient.EditMessageTextAsync(
                callback.Message.Chat,
                callback.Message.MessageId,
                LocalizationProvider.GetMessage(CallbackMessageKeys.EnqueueAtCallbackHandler.Callback_EnqueueAt_QueueIsFull_Message, new MessageParameters(ex.QueueName)),
                ParseMode.Html,
                replyMarkup: GetReturnToQueueButton(callback.CallbackData),
                cancellationToken: cancellationToken);
        }
        catch (PositionIsReservedException ex)
        {
            await TelegramBotClient.EditMessageTextAsync(
                callback.Message.Chat,
                callback.Message.MessageId,
                LocalizationProvider.GetMessage(CallbackMessageKeys.EnqueueAtCallbackHandler.Callback_EnqueueAt_PositionIsReserved_Message, new MessageParameters(ex.QueueName, ex.Position.ToString())),
                ParseMode.Html,
                replyMarkup: GetQueueRelatedButton(LocalizationProvider.GetMessage(CallbackMessageKeys.EnqueueAtCallbackHandler.Callback_EnqueueAt_PositionIsReserved_ChooseAnother_Button, MessageParameters.None), CallbackCommands.EnqueueCommand, callback.CallbackData, queueId),
                cancellationToken: cancellationToken);
        }
        catch (QueueIsDynamicException ex)
        {
            await TelegramBotClient.EditMessageTextAsync(
                callback.Message.Chat,
                callback.Message.MessageId,
                LocalizationProvider.GetMessage(CallbackMessageKeys.EnqueueAtCallbackHandler.Callback_EnqueueAt_QueueIsDynamicButPositionIsSpecified_Message, new MessageParameters(ex.QueueName)),
                ParseMode.Html,
                replyMarkup: new InlineKeyboardMarkup(new InlineKeyboardButton[]
                {
                    GetQueueRelatedButton(LocalizationProvider.GetMessage(CallbackMessageKeys.EnqueueCallbackHandler.Callback_Enqueue_FirstAvailable_Button, MessageParameters.None), CallbackCommands.EnqueueAtCommand, callback.CallbackData, queueId),
                    GetReturnToQueueButton(callback.CallbackData)
                }),
                cancellationToken: cancellationToken);
        }
    }
}
