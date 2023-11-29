using System.Threading;
using System.Threading.Tasks;
using Enqueuer.Messaging.Core;
using Enqueuer.Messaging.Core.Localization;
using Enqueuer.Messaging.Core.Serialization;
using Enqueuer.Messaging.Core.Types.Callbacks;
using Enqueuer.Persistence.Models;
using Enqueuer.Services;
using Enqueuer.Services.Extensions;
using Enqueuer.Telegram.Callbacks.Helpers;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Enqueuer.Telegram.Callbacks.CallbackHandlers;

public class EnqueueCallbackHandler : CallbackHandlerBase
{
    private const int PositionsToDisplay = 20;
    private const int PositionsInRow = 4;
    private readonly ICallbackDataSerializer _dataSerializer;
    private readonly IQueueService _queueService;

    public EnqueueCallbackHandler(ITelegramBotClient telegramBotClient, ICallbackDataSerializer dataSerializer, ILocalizationProvider localizationProvider, IQueueService queueService)
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
        var queue = await _queueService.GetQueueAsync(queueId, includeMembers: true, cancellationToken);
        if (queue == null)
        {
            var replyMarkup = ReplyMarkupBuilder.Create(_dataSerializer, LocalizationProvider)
                .WithReturnToChatButton(callbackContext.CallbackData)
                .Build();

            await TelegramBotClient.EditMessageTextAsync(
                callbackContext.Chat.Id,
                callbackContext.MessageId,
                LocalizationProvider.GetMessage(CallbackMessageKeys.EnqueueCallbackHandler.Callback_Enqueue_QueueHasBeenDeleted_Message, MessageParameters.None),
                replyMarkup: replyMarkup,
                cancellationToken: cancellationToken);

            return;
        }

        await HandleCallbackWithExistingQueueAsync(queue, callbackContext, cancellationToken);
    }

    private async Task HandleCallbackWithExistingQueueAsync(Queue queue, CallbackContext callbackContext, CancellationToken cancellationToken)
    {
        InlineKeyboardMarkup replyButtons;
        if (queue.IsDynamic)
        {
            replyButtons = ReplyMarkupBuilder.Create(_dataSerializer, LocalizationProvider)
                .WithEnqueueAtButton(callbackContext.CallbackData, LocalizationProvider.GetMessage(CallbackMessageKeys.EnqueueCallbackHandler.Callback_Enqueue_FirstAvailable_Button, MessageParameters.None))
                .FromNewRow()
                .WithReturnToQueueButton(callbackContext.CallbackData)
                .Build();
        }
        else
        {
            var availablePositions = queue.GetAvailablePositions(PositionsToDisplay);
            replyButtons = BuildKeyboardMarkup(availablePositions, callbackContext.CallbackData);
        }

        await TelegramBotClient.EditMessageTextAsync(
            callbackContext.Chat.Id,
            callbackContext.MessageId,
            LocalizationProvider.GetMessage(CallbackMessageKeys.EnqueueCallbackHandler.Callback_Enqueue_SelectPosition_Message, new MessageParameters(queue.Name)),
            ParseMode.Html,
            replyMarkup: replyButtons,
            cancellationToken: cancellationToken);
    }

    private InlineKeyboardMarkup BuildKeyboardMarkup(int[] availablePositions, CallbackData callbackData)
    {
        var replyMarkup = ReplyMarkupBuilder.Create(_dataSerializer, LocalizationProvider)
            .WithEnqueueAtButton(
                callbackData,
                LocalizationProvider.GetMessage(CallbackMessageKeys.EnqueueCallbackHandler.Callback_Enqueue_FirstAvailable_Button, MessageParameters.None));

        var numberOfRows = availablePositions.Length / PositionsInRow;
        AddPositionButtons(availablePositions, replyMarkup, numberOfRows, callbackData);

        replyMarkup.FromNewRow()
            .WithRefreshButton(callbackData)
            .FromNewRow()
            .WithReturnToQueueButton(callbackData);

        return replyMarkup.Build();
    }

    private void AddPositionButtons(int[] availablePositions, ReplyMarkupBuilder markupBuilder, int numberOfRows, CallbackData callbackData)
    {
        for (int i = 1, positionIndex = 0; i < numberOfRows + 1; i++)
        {
            markupBuilder.FromNewRow();
            for (var j = 0; j < PositionsInRow; j++, positionIndex++)
            {
                var position = availablePositions[positionIndex];
                markupBuilder.WithEnqueueAtButton(callbackData, position: position);
            }
        }
    }
}
