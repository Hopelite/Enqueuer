using System.Collections.Generic;
using System.Threading.Tasks;
using Enqueuer.Data;
using Enqueuer.Data.Constants;
using Enqueuer.Data.DataSerialization;
using Enqueuer.Callbacks.CallbackHandlers.BaseClasses;
using Enqueuer.Callbacks.Exceptions;
using Enqueuer.Persistence.Models;
using Enqueuer.Services.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Enqueuer.Callbacks.CallbackHandlers
{
    /// <inheritdoc/>
    public class EnqueueCallbackHandler : CallbackHandlerBaseWithReturnToQueueButton
    {
        private const int PositionsInRow = 4;
        private readonly IQueueService queueService;
        private readonly IUserInQueueService userInQueueService;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetQueueCallbackHandler"/> class.
        /// </summary>
        /// <param name="queueService">Queue service to use.</param>
        /// <param name="userInQueueService">User in queue service to use.</param>
        /// <param name="dataSerializer"><see cref="IDataSerializer"/> to serialize with.</param>
        public EnqueueCallbackHandler(IQueueService queueService, IUserInQueueService userInQueueService, IDataSerializer dataSerializer)
            : base(dataSerializer)
        {
            this.queueService = queueService;
            this.userInQueueService = userInQueueService;
        }

        /// <inheritdoc/>
        public override string Command => CallbackConstants.EnqueueCommand;

        /// <inheritdoc/>
        public override async Task<Message> HandleCallbackAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, CallbackData callbackData)
        {
            if (callbackData.QueueData is not null)
            {
                var queue = this.queueService.GetQueueById(callbackData.QueueData.QueueId);
                if (queue is null)
                {
                    return await botClient.EditMessageTextAsync(
                        callbackQuery.Message.Chat,
                        callbackQuery.Message.MessageId,
                        "This queue has been deleted.",
                        replyMarkup: this.GetReturnToChatButton(callbackData));
                }

                return await HandleCallbackWithExistingQueueAsync(botClient, callbackQuery, queue, callbackData);
            }

            throw new CallbackMessageHandlingException("Null queue data passed to message handler.");
        }

        private async Task<Message> HandleCallbackWithExistingQueueAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, Queue queue, CallbackData callbackData)
        {
            var availablePositions = this.userInQueueService.GetAvailablePositions(queue) as List<int>;
            var replyButtons = this.BuildKeyboardMarkup(availablePositions, callbackData);
            return await botClient.EditMessageTextAsync(
                callbackQuery.Message.Chat,
                callbackQuery.Message.MessageId,
                $"Select an available position in queue <b>'{queue.Name}'</b>:",
                ParseMode.Html,
                replyMarkup: replyButtons);
        }

        private InlineKeyboardMarkup BuildKeyboardMarkup(List<int> availablePositions, CallbackData callbackData)
        {
            var numberOfRows = availablePositions.Count / PositionsInRow;
            var positionButtons = new InlineKeyboardButton[numberOfRows + 2][];

            positionButtons[0] = new InlineKeyboardButton[] { this.GetEnqueueAtButton(callbackData) };
            this.AddPositionButtons(availablePositions, positionButtons, numberOfRows, callbackData);
            positionButtons[numberOfRows + 1] = new InlineKeyboardButton[] { this.GetReturnToQueueButton(callbackData) };

            return new InlineKeyboardMarkup(positionButtons);
        }

        private void AddPositionButtons(List<int> availablePositions, InlineKeyboardButton[][] positionButtons, int numberOfRows, CallbackData callbackData)
        {
            for (int i = 1, positionIndex = 0; i < numberOfRows + 1; i++)
            {
                positionButtons[i] = new InlineKeyboardButton[PositionsInRow];
                for (int j = 0; j < PositionsInRow; j++, positionIndex++)
                {
                    var position = availablePositions[positionIndex];
                    positionButtons[i][j] = this.GetEnqueueAtButton(callbackData, position);
                }
            }
        }

        private InlineKeyboardButton GetEnqueueAtButton(CallbackData callbackData, int? position = null)
        {
            var buttonCallbackData = new CallbackData()
            {
                Command = CallbackConstants.EnqueueAtCommand,
                ChatId = callbackData.ChatId,
                QueueData = new QueueData()
                {
                    QueueId = callbackData.QueueData.QueueId,
                    Position = position,
                },
            };

            var serializedCallbackData = this.dataSerializer.Serialize(buttonCallbackData);
            return InlineKeyboardButton.WithCallbackData($"{position}", serializedCallbackData);
        }
    }
}
