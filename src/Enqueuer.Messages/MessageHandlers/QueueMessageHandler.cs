using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Enqueuer.Data.Constants;
using Enqueuer.Persistence.Models;
using Enqueuer.Services.Interfaces;
using Enqueuer.Messages.Extensions;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Chat = Enqueuer.Persistence.Models.Chat;

namespace Enqueuer.Messages.MessageHandlers
{
    /// <inheritdoc/>
    public class QueueMessageHandler : MessageHandlerBase
    {
        private readonly IQueueService queueService;

        /// <summary>
        /// Initializes a new instance of the <see cref="QueueMessageHandler"/> class.
        /// </summary>
        /// <param name="chatService">Chat service to use.</param>
        /// <param name="userService">User service to use.</param>
        /// <param name="queueService">Queue service to use.</param>
        public QueueMessageHandler(
            IChatService chatService,
            IUserService userService,
            IQueueService queueService)
            : base(chatService, userService)
        {
            this.queueService = queueService;
        }

        /// <inheritdoc/>
        public override string Command => MessageConstants.QueueCommand;

        /// <inheritdoc/>
        public override async Task<Message> HandleMessageAsync(ITelegramBotClient botClient, Message message)
        {
            if (message.IsPrivateChat())
            {
                return await botClient.SendUnsupportedOperationMessage(message);
            }

            var (_, chat) = await this.GetNewOrExistingUserAndChat(message);
            var messageWords = message.Text.SplitToWords();
            if (messageWords.HasParameters())
            {
                return await this.HandleMessageWithParameters(botClient, message, messageWords, chat);
            }

            return await this.HandleMessageWithoutParameters(botClient, message, chat);
        }

        private async Task<Message> HandleMessageWithParameters(ITelegramBotClient botClient, Message message, string[] messageWords, Chat chat)
        {
            var queueName = messageWords.GetQueueName();
            var queue = this.queueService.GetChatQueueByName(queueName, chat.ChatId);
            if (queue is null)
            {
                return await botClient.SendTextMessageAsync(
                    chat.ChatId,
                    $"This chat has no queue with name '<b>{queueName}</b>'.",
                    ParseMode.Html,
                    replyToMessageId: message.MessageId);
            }

            if (queue.Users.Count == 0)
            {
                return await botClient.SendTextMessageAsync(
                    chat.ChatId,
                    $"Queue '<b>{queue.Name}</b>' has no participants.",
                    ParseMode.Html);
            }

            var responseMessage = BuildResponseMessageWithQueueParticipants(queue);
            return await botClient.SendTextMessageAsync(
                chat.ChatId,
                responseMessage,
                ParseMode.Html);
        }

        private async Task<Message> HandleMessageWithoutParameters(ITelegramBotClient botClient, Message message, Chat chat)
        {
            var chatQueues = this.queueService.GetTelegramChatQueues(chat.ChatId);
            if (!chatQueues.Any())
            {
                return await botClient.SendTextMessageAsync(
                        chat.ChatId,
                        $"This chat has no queues. To create new one write '<b>/createqueue</b> <i>queue_name</i>'.",
                        ParseMode.Html,
                        replyToMessageId: message.MessageId);
            }

            var replyMessage = BuildResponseMessageWithChatQueues(chatQueues);
            return await botClient.SendTextMessageAsync(
                chat.ChatId,
                replyMessage,
                ParseMode.Html);
        }

        private static string BuildResponseMessageWithChatQueues(IEnumerable<Queue> chatQueues)
        {
            var replyMessage = new StringBuilder("This chat has these queues:\n");
            foreach (var queue in chatQueues)
            {
                replyMessage.AppendLine($"• {queue.Name}");
            }

            replyMessage.AppendLine("To get info about one of them write '<b>/queue</b> <i>[queue_name]</i>'.");
            return replyMessage.ToString();
        }

        private static string BuildResponseMessageWithQueueParticipants(Queue queue)
        {
            var responseMessage = new StringBuilder($"'<b>{queue.Name}</b>' has these participants:\n");
            var queueParticipants = queue.Users.OrderBy(queueUser => queueUser.Position)
                .Select(queueUser => (queueUser.Position, queueUser.User));
            foreach (var queueParticipant in queueParticipants)
            {
                responseMessage.AppendLine($"{queueParticipant.Position}) <b>{queueParticipant.User.FirstName} {queueParticipant.User.LastName}</b>");
            }

            return responseMessage.ToString();
        }
    }
}
