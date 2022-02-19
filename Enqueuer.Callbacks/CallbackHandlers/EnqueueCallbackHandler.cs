using System.Collections.Generic;
using System.Threading.Tasks;
using Enqueuer.Callbacks.Exceptions;
using Enqueuer.Persistence.Models;
using Enqueuer.Services.Interfaces;
using Enqueuer.Utilities.Extensions;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Enqueuer.Callbacks.CallbackHandlers
{
    /// <inheritdoc/>
    public class EnqueueCallbackHandler : ICallbackHandler
    {
        private const int PositionsInRow = 4;
        private readonly IQueueService queueService;
        private readonly IUserInQueueService userInQueueService;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetQueueCallbackHandler"/> class.
        /// </summary>
        /// <param name="queueService">Queue service to use.</param>
        /// <param name="userInQueueService">User in queue service to use.</param>
        public EnqueueCallbackHandler(IQueueService queueService, IUserInQueueService userInQueueService)
        {
            this.queueService = queueService;
            this.userInQueueService = userInQueueService;
        }
        
        /// <inheritdoc/>
        public string Command => "/enqueue";

        /// <inheritdoc/>
        public async Task<Message> HandleCallbackAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery)
        {
            var callbackData = callbackQuery.Data.SplitToWords();
            if (int.TryParse(callbackData[1], out var queueId))
            {
                if (long.TryParse(callbackData[2], out var chatId))
                {
                    var queue = this.queueService.GetQueueById(queueId);
                    if (queue is null)
                    {
                        var returnButton = InlineKeyboardButton.WithCallbackData("Return", $"/getchat {chatId}");
                        return await botClient.EditMessageTextAsync(
                            callbackQuery.Message.Chat,
                            callbackQuery.Message.MessageId,
                            "This queue has been deleted.",
                            replyMarkup: returnButton);
                    }

                    return await HandleCallbackWithExistingQueueAsync(botClient, callbackQuery, queue, queueId, chatId);
                }

                throw new CallbackMessageHandlingException("Invalid chat ID passed to message handler.");
            }

            throw new CallbackMessageHandlingException("Invalid queue ID passed to message handler.");
        }

        private async Task<Message> HandleCallbackWithExistingQueueAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, Queue queue, int queueId, long chatId)
        {
            var availablePositions = this.userInQueueService.GetAvailablePositions(queue) as List<int>;
            var replyButtons = BuildKeyboardMarkup(availablePositions, queueId, chatId);
            return await botClient.EditMessageTextAsync(
                callbackQuery.Message.Chat,
                callbackQuery.Message.MessageId,
                $"Select available position in queue <b>'{queue.Name}'</b>:",
                ParseMode.Html,
                replyMarkup: replyButtons);
        }

        private static InlineKeyboardMarkup BuildKeyboardMarkup(List<int> availablePositions, int queueId, long chatId)
        {
            var numberOfRows = availablePositions.Count / PositionsInRow;
            var positionButtons = new InlineKeyboardButton[numberOfRows + 2][];

            positionButtons[0] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData("First available", $"/enqueueat {queueId}") };
            AddPositionButtons(availablePositions, positionButtons, numberOfRows, queueId);
            positionButtons[numberOfRows + 1] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData("Return", $"/getqueue {queueId} {chatId}") };

            return new InlineKeyboardMarkup(positionButtons);
        }

        private static void AddPositionButtons(List<int> availablePositions, InlineKeyboardButton[][] positionButtons, int numberOfRows, int queueId)
        {
            for (int i = 1, positionIndex = 0; i < numberOfRows + 1; i++)
            {
                positionButtons[i] = new InlineKeyboardButton[PositionsInRow];
                for (int j = 0; j < PositionsInRow; j++, positionIndex++)
                {
                    var position = availablePositions[positionIndex];
                    positionButtons[i][j] = InlineKeyboardButton.WithCallbackData($"{position}", $"/enqueueat {position} {queueId}");
                }
            }
        }
    }
}
