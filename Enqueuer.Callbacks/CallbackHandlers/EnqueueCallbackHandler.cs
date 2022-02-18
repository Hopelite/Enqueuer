using System.Collections.Generic;
using System.Threading.Tasks;
using Enqueuer.Services.Interfaces;
using Enqueuer.Utilities.Extensions;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetQueueCallbackHandler"/> class.
        /// </summary>
        /// <param name="queueService">Queue service to use.</param>
        /// <param name="userInQueueService">User in queue service to use.</param>
        /// <param name="logger">Logger to log errors.</param>
        public EnqueueCallbackHandler(IQueueService queueService, IUserInQueueService userInQueueService, ILogger logger)
        {
            this.queueService = queueService;
            this.userInQueueService = userInQueueService;
            this.logger = logger;
        }
        
        /// <inheritdoc/>
        public string Command => "/enqueue";

        /// <summary>
        /// Handles incoming <paramref name="callbackQuery"/> with '/enqueue' command.
        /// </summary>
        /// <param name="botClient"><see cref="ITelegramBotClient"/> to use.</param>
        /// <param name="callbackQuery">Incoming <see cref="CallbackQuery"/> to handle.</param>
        /// <returns><see cref="Message"/> which was sent in responce.</returns>
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

                    var availablePositions = this.userInQueueService.GetAvailablePositions(queue) as List<int>;
                    var numberOfRows = availablePositions.Count / PositionsInRow;
                    var positionButtons = new InlineKeyboardButton[numberOfRows + 2][];
                    positionButtons[0] = new InlineKeyboardButton[1];
                    positionButtons[0][0] = InlineKeyboardButton.WithCallbackData("First available", $"/enqueueat {queueId}");
                    for (int i = 1, positionIndex = 0; i < numberOfRows + 1; i++)
                    {
                        positionButtons[i] = new InlineKeyboardButton[PositionsInRow];
                        for (int j = 0; j < PositionsInRow; j++, positionIndex++)
                        {
                            var position = availablePositions[positionIndex];
                            positionButtons[i][j] = InlineKeyboardButton.WithCallbackData($"{position}", $"/enqueueat {position} {queueId}");
                        }
                    }

                    positionButtons[numberOfRows + 1] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData("Return", $"/getqueue {queueId} {chatId}") };
                    return await botClient.EditMessageTextAsync(
                        callbackQuery.Message.Chat,
                        callbackQuery.Message.MessageId,
                        $"Select available position in queue <b>'{queue.Name}'</b>:",
                        ParseMode.Html,
                        replyMarkup: new InlineKeyboardMarkup(positionButtons));
                }

                this.logger.LogError("Invalid chat ID passed to message handler.");
                return null;
            }

            this.logger.LogError("Invalid queue ID passed to message handler.");
            return null;
        }
    }
}
