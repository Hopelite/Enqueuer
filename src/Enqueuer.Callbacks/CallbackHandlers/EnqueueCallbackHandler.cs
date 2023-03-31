using System;
using System.Threading;
using System.Threading.Tasks;
using Enqueuer.Callbacks.CallbackHandlers.BaseClasses;
using Enqueuer.Data;
using Enqueuer.Data.Constants;
using Enqueuer.Data.DataSerialization;
using Enqueuer.Data.TextProviders;
using Enqueuer.Persistence.Models;
using Enqueuer.Services;
using Enqueuer.Services.Extensions;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Enqueuer.Callbacks.CallbackHandlers;

public class EnqueueCallbackHandler : CallbackHandlerBaseWithReturnToQueueButton
{
    private const int PositionsToDisplay = 20;
    private const int PositionsInRow = 4;
    private readonly IQueueService _queueService;
    private readonly ILogger<EnqueueCallbackHandler> _logger;

    public EnqueueCallbackHandler(ITelegramBotClient telegramBotClient, IDataSerializer dataSerializer, IMessageProvider messageProvider, IQueueService queueService, ILogger<EnqueueCallbackHandler> logger)
        : base(telegramBotClient, dataSerializer, messageProvider)
    {
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
        var queue = await _queueService.GetQueueAsync(callback.CallbackData.QueueData.QueueId, includeMembers: true, CancellationToken.None);
        if (queue == null)
        {
            await TelegramBotClient.EditMessageTextAsync(
                callback.Message.Chat,
                callback.Message.MessageId,
                MessageProvider.GetMessage(CallbackMessageKeys.EnqueueCallbackHandler.EnqueueCallback_QueueHasBeenDeleted_Message),
                replyMarkup: GetReturnToChatButton(callback.CallbackData));

            return;
        }

        await HandleCallbackWithExistingQueueAsync(queue, callback);
    }

    private async Task HandleCallbackWithExistingQueueAsync(Queue queue, Callback callback)
    {
        InlineKeyboardMarkup replyButtons;
        if (queue.IsDynamic)
        {
            replyButtons = new InlineKeyboardMarkup(new InlineKeyboardButton[2][]
            {
                new InlineKeyboardButton[] { GetEnqueueAtButton(callback.CallbackData, MessageProvider.GetMessage(CallbackMessageKeys.EnqueueCallbackHandler.EnqueueCallback_FirstAvailable_Button)) },
                new InlineKeyboardButton[] {  GetReturnToQueueButton(callback.CallbackData) }
            });
        }
        else
        {
            var availablePositions = queue.GetAvailablePositions(PositionsToDisplay);
            replyButtons = BuildKeyboardMarkup(availablePositions, callback.CallbackData);
        }

        await TelegramBotClient.EditMessageTextAsync(
            callback.Message.Chat,
            callback.Message.MessageId,
            MessageProvider.GetMessage(CallbackMessageKeys.EnqueueCallbackHandler.EnqueueCallback_SelectPosition_Message, queue.Name),
            ParseMode.Html,
            replyMarkup: replyButtons);
    }

    private InlineKeyboardMarkup BuildKeyboardMarkup(int[] availablePositions, CallbackData callbackData)
    {
        var numberOfRows = availablePositions.Length / PositionsInRow;
        var positionButtons = new InlineKeyboardButton[numberOfRows + 3][];

        positionButtons[0] = new InlineKeyboardButton[] { GetEnqueueAtButton(callbackData, MessageProvider.GetMessage(CallbackMessageKeys.EnqueueCallbackHandler.EnqueueCallback_FirstAvailable_Button)) };
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
            for (int j = 0; j < PositionsInRow; j++, positionIndex++)
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
            Command = CallbackConstants.EnqueueAtCommand,
            TargetChatId = callbackData.TargetChatId,
            QueueData = new QueueData()
            {
                QueueId = callbackData.QueueData.QueueId,
                Position = position,
            },
        };

        var serializedCallbackData = DataSerializer.Serialize(buttonCallbackData);
        return InlineKeyboardButton.WithCallbackData($"{buttonText ?? position?.ToString() ?? throw new ArgumentNullException(nameof(buttonText)) }", serializedCallbackData);
    }
}
