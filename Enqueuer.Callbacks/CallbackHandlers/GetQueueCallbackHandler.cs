using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Enqueuer.Callbacks.CallbackHandlers.BaseClasses;
using Enqueuer.Callbacks.Exceptions;
using Enqueuer.Data;
using Enqueuer.Data.Constants;
using Enqueuer.Data.DataSerialization;
using Enqueuer.Persistence.Extensions;
using Enqueuer.Persistence.Models;
using Enqueuer.Services.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using User = Enqueuer.Persistence.Models.User;

namespace Enqueuer.Callbacks.CallbackHandlers
{
    /// <inheritdoc/>
    public class GetQueueCallbackHandler : CallbackHandlerBaseWithRemoveQueueButton
    {
        private readonly IQueueService queueService;
        private readonly IUserService userService;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetQueueCallbackHandler"/> class.
        /// </summary>
        /// <param name="queueService">Queue service to use.</param>
        /// <param name="userService">User service to use.</param>
        public GetQueueCallbackHandler(IQueueService queueService, IUserService userService, IDataSerializer dataSerializer)
            : base(dataSerializer)
        {
            this.queueService = queueService;
            this.userService = userService;
        }

        /// <inheritdoc/>
        public override string Command => CallbackConstants.GetQueueCommand;

        /// <inheritdoc/>
        public override async Task<Message> HandleCallbackAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, CallbackData callbackData)
        {
            if (callbackData.QueueData is not null)
            {
                var queue = this.queueService.GetQueueById(callbackData.QueueData.QueueId);
                if (queue is null)
                {
                    var returnButton = this.GetReturnToChatButton(callbackData);
                    return await botClient.EditMessageTextAsync(
                        callbackQuery.Message.Chat,
                        callbackQuery.Message.MessageId,
                        "This queue has been deleted.",
                        replyMarkup: returnButton);
                }

                return await this.HandleCallbackWithExistingQueueAsync(botClient, callbackQuery, queue, callbackData);
            }

            throw new CallbackMessageHandlingException("Null queue data passed to callback handler.");
        }

        private async Task<Message> HandleCallbackWithExistingQueueAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, Queue queue, CallbackData callbackData)
        {
            var user = this.userService.GetUserByUserId(callbackQuery.From.Id);
            var replyMarkup = this.BuildReplyMarkup(user, queue, callbackData);
            var responceMessage = BuildResponceMessage(queue);
            return await botClient.EditMessageTextAsync(
                callbackQuery.Message.Chat,
                callbackQuery.Message.MessageId,
                responceMessage,
                ParseMode.Html,
                replyMarkup: replyMarkup);
        }

        private InlineKeyboardMarkup BuildReplyMarkup(User user, Queue queue, CallbackData callbackData)
        {
            var replyMarkupButtons = new List<InlineKeyboardButton[]>()
            {
                user.IsParticipatingIn(queue)
                ? new InlineKeyboardButton[] { this.GetQueueRelatedButton("Dequeue me", CallbackConstants.DequeueMeCommand, callbackData, queue.Id) }
                : new InlineKeyboardButton[] { this.GetQueueRelatedButton("Enqueue me", CallbackConstants.EnqueueCommand, callbackData, queue.Id) }
            };

            if (IsUserQueueCreator(user, queue))
            {
                replyMarkupButtons.Add(new InlineKeyboardButton[] { this.GetRemoveQueueButton("Remove queue", callbackData) });
            }

            replyMarkupButtons.Add(new InlineKeyboardButton[] { this.GetReturnToChatButton(callbackData) });
            return new InlineKeyboardMarkup(replyMarkupButtons);
        }

        private InlineKeyboardButton GetQueueRelatedButton(string buttonText, string command, CallbackData callbackData, int queueId)
        {
            var buttonCallbackData = new CallbackData()
            {
                Command = command,
                ChatId = callbackData.ChatId,
                QueueData = new QueueData()
                { 
                    QueueId = queueId,
                }
            };

            var serializedCallbackData = this.dataSerializer.Serialize(buttonCallbackData);
            return InlineKeyboardButton.WithCallbackData(buttonText, serializedCallbackData);
        }

        private static string BuildResponceMessage(Queue queue)
        {
            StringBuilder responceMessage;
            if (queue.Users.Count() == 0)
            {
                responceMessage = new StringBuilder($"Queue <b>'{queue.Name}'</b> has no participants.");
            }
            else
            {
                responceMessage = new StringBuilder($"Queue <b>'{queue.Name}'</b> has these participants:\n");
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
