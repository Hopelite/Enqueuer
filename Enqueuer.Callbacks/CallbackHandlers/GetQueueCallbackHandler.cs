using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    public class GetQueueCallbackHandler : ICallbackHandler
    {
        private readonly IQueueService queueService;
        private readonly IUserService userService;
        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetQueueCallbackHandler"/> class.
        /// </summary>
        /// <param name="queueService">Queue service to use.</param>
        /// <param name="userService">User service to use.</param>
        /// <param name="logger">Logger to log errors.</param>
        public GetQueueCallbackHandler(IQueueService queueService, IUserService userService, ILogger logger)
        {
            this.queueService = queueService;
            this.userService = userService;
            this.logger = logger;
        }

        /// <inheritdoc/>
        public string Command => "/getqueue";

        /// <summary>
        /// Handles incoming <paramref name="callbackQuery"/> with '/getqueue' command.
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
                            chatId,
                            callbackQuery.Message.MessageId,
                            "This queue has been deleted.",
                            replyMarkup: returnButton);
                    }

                    StringBuilder responceMessage;
                    var replyMarkupButtons = new List<InlineKeyboardButton>();
                    if (queue.Users.Count() == 0)
                    {
                        responceMessage = new StringBuilder("This queue has no participants.");
                    }
                    else
                    {
                        responceMessage = new StringBuilder("This queue has these participants:\n");
                        foreach (var queueParticipant in queue.Users)
                        {
                            responceMessage.AppendLine($"{queueParticipant.Position}) <b>{queueParticipant.User.FirstName} {queueParticipant.User.LastName}</b>");
                        }
                    }

                    var user = this.userService.GetUserByUserId(callbackQuery.From.Id);
                    if (queue.Users.FirstOrDefault(user => user.UserId == user.Id) is not null)
                    {
                        replyMarkupButtons.Add(InlineKeyboardButton.WithCallbackData("Dequeue me", $"/dequeue {queue.Id}"));
                    }
                    else
                    {
                        replyMarkupButtons.Add(InlineKeyboardButton.WithCallbackData("Enqueue me", $"/enqueue {queue.Id}"));
                    }

                    if (queue.CreatorId == user.Id)
                    {
                        replyMarkupButtons.Add(InlineKeyboardButton.WithCallbackData("Delete queue", $"/removequeue {queue.Id}"));
                    }

                    replyMarkupButtons.Add(InlineKeyboardButton.WithCallbackData("Return", $"/getchat {chatId}"));
                    return await botClient.EditMessageTextAsync(
                        callbackQuery.Message.Chat,
                        callbackQuery.Message.MessageId,
                        responceMessage.ToString(),
                        ParseMode.Html,
                        replyMarkup: new InlineKeyboardMarkup(replyMarkupButtons));
                }

                this.logger.LogError("Invalid chat ID passed to message handler.");
                return null;
            }

            this.logger.LogError("Invalid queue ID passed to message handler.");
            return null;
        }
    }
}
