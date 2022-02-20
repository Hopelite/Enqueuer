using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Enqueuer.Callbacks.Exceptions;
using Enqueuer.Persistence.Extensions;
using Enqueuer.Persistence.Models;
using Enqueuer.Services.Interfaces;
using Enqueuer.Utilities.Extensions;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using User = Enqueuer.Persistence.Models.User;

namespace Enqueuer.Callbacks.CallbackHandlers
{
    /// <inheritdoc/>
    public class GetQueueCallbackHandler : ICallbackHandler
    {
        private readonly IQueueService queueService;
        private readonly IUserService userService;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetQueueCallbackHandler"/> class.
        /// </summary>
        /// <param name="queueService">Queue service to use.</param>
        /// <param name="userService">User service to use.</param>
        public GetQueueCallbackHandler(IQueueService queueService, IUserService userService)
        {
            this.queueService = queueService;
            this.userService = userService;
        }

        /// <inheritdoc/>
        public string Command => "/getqueue";

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

                    return await this.HandleCallbackWithExistingQueueAsync(botClient, callbackQuery, queue, chatId);
                }

                throw new CallbackMessageHandlingException("Invalid chat ID passed to message handler.");
            }

            throw new CallbackMessageHandlingException("Invalid queue ID passed to message handler.");
        }

        private async Task<Message> HandleCallbackWithExistingQueueAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, Queue queue, long chatId)
        {
            var user = this.userService.GetUserByUserId(callbackQuery.From.Id);
            var replyMarkup = BuildReplyMarkup(user, queue, chatId);
            var responceMessage = BuildResponceMessage(queue);
            return await botClient.EditMessageTextAsync(
                callbackQuery.Message.Chat,
                callbackQuery.Message.MessageId,
                responceMessage,
                ParseMode.Html,
                replyMarkup: replyMarkup);
        }

        private static InlineKeyboardMarkup BuildReplyMarkup(User user, Queue queue, long chatId)
        {
            var replyMarkupButtons = new List<InlineKeyboardButton[]>()
            {
                user.IsParticipatingIn(queue)
                ? new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData("Dequeue me", $"/dequeueme {queue.Id}") }
                : new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData("Enqueue me", $"/enqueue {queue.Id} {chatId}") }
            };

            if (IsUserQueueCreator(user, queue))
            {
                replyMarkupButtons.Add(new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData("Remove queue", $"/removequeue {queue.Id}") });
            }
            replyMarkupButtons.Add(new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData("Return", $"/getchat {chatId}") });

            return new InlineKeyboardMarkup(replyMarkupButtons);
        }

        private static string BuildResponceMessage(Queue queue)
        {
            StringBuilder responceMessage;
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

            return responceMessage.ToString();
        }

        private static bool IsUserQueueCreator(User user, Queue queue)
        {
            return queue.CreatorId == user.Id;
        }
    }
}
