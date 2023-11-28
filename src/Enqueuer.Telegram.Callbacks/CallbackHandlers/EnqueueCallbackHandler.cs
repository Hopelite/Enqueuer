using System;
using System.Threading;
using System.Threading.Tasks;
using Enqueuer.Persistence.Models;
using Enqueuer.Services;
using Enqueuer.Services.Extensions;
using Enqueuer.Telegram.Callbacks.CallbackHandlers.BaseClasses;
using Enqueuer.Messaging.Core;
using Enqueuer.Messaging.Core.Constants;
using Enqueuer.Messaging.Core.Localization;
using Enqueuer.Messaging.Core.Serialization;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Enqueuer.Messaging.Core.Types.Callbacks;

namespace Enqueuer.Telegram.Callbacks.CallbackHandlers;

public class EnqueueCallbackHandler : CallbackHandlerBaseWithReturnToQueueButton
{
    private const int PositionsToDisplay = 20;
    private const int PositionsInRow = 4;
    private readonly IQueueService _queueService;

    public EnqueueCallbackHandler(ITelegramBotClient telegramBotClient, ICallbackDataSerializer dataSerializer, ILocalizationProvider localizationProvider, IQueueService queueService)
        : base(telegramBotClient, dataSerializer, localizationProvider)
    {
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
            await TelegramBotClient.EditMessageTextAsync(
                callbackContext.Chat.Id,
                callbackContext.MessageId,
                LocalizationProvider.GetMessage(CallbackMessageKeys.EnqueueCallbackHandler.Callback_Enqueue_QueueHasBeenDeleted_Message, MessageParameters.None),
                replyMarkup: GetReturnToChatButton(callbackContext.CallbackData),
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
            replyButtons = new InlineKeyboardMarkup(new InlineKeyboardButton[2][]
            {
                new InlineKeyboardButton[] { GetEnqueueAtButton(callbackContext.CallbackData, LocalizationProvider.GetMessage(CallbackMessageKeys.EnqueueCallbackHandler.Callback_Enqueue_FirstAvailable_Button, MessageParameters.None)) },
                new InlineKeyboardButton[] {  GetReturnToQueueButton(callbackContext.CallbackData) }
            });
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
        var numberOfRows = availablePositions.Length / PositionsInRow;
        var positionButtons = new InlineKeyboardButton[numberOfRows + 3][];

        positionButtons[0] = new InlineKeyboardButton[] { GetEnqueueAtButton(callbackData, LocalizationProvider.GetMessage(CallbackMessageKeys.EnqueueCallbackHandler.Callback_Enqueue_FirstAvailable_Button, MessageParameters.None)) };
        AddPositionButtons(availablePositions, positionButtons, numberOfRows, callbackData);
        positionButtons[numberOfRows + 1] = new InlineKeyboardButton[] { GetRefreshButton(callbackData) };
        positionButtons[numberOfRows + 2] = new InlineKeyboardButton[] { GetReturnToQueueButton(callbackData) };

        return new InlineKeyboardMarkup(positionButtons);
    }

    private void AddPositionButtons(int[] availablePositions, InlineKeyboardButton[][] positionButtons, int numberOfRows, CallbackData callbackData)
    {
        for (int i = 1, positionIndex = 0; i < numberOfRows + 1; i++)
        {
            positionButtons[i] = new InlineKeyboardButton[PositionsInRow];
            for (var j = 0; j < PositionsInRow; j++, positionIndex++)
            {
                var position = availablePositions[positionIndex];
                positionButtons[i][j] = GetEnqueueAtButton(callbackData, position: position);
            }
        }
    }

    private InlineKeyboardButton GetEnqueueAtButton(CallbackData callbackData, string? buttonText = null, int? position = null)
    {
        var buttonCallbackData = new CallbackData()
        {
            Command = CallbackCommands.EnqueueAtCommand,
            TargetChatId = callbackData.TargetChatId,
            QueueData = new QueueData()
            {
                QueueId = callbackData.QueueData!.QueueId,
                Position = position,
            },
        };

        var serializedCallbackData = DataSerializer.Serialize(buttonCallbackData);
        return InlineKeyboardButton.WithCallbackData($"{buttonText ?? position?.ToString() ?? throw new ArgumentNullException(nameof(buttonText))}", serializedCallbackData);
    }
}
