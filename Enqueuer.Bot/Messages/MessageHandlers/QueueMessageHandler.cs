using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Enqueuer.Bot.Extensions;
using Enqueuer.Services.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Types;
using Chat = Enqueuer.Persistence.Models.Chat;

namespace Enqueuer.Bot.Messages.MessageHandlers
{
    /// <summary>
    /// Handles incoming <see cref="Message"/> with '/createqueue' command.
    /// </summary>
    public class QueueMessageHandler : IMessageHandler
    {
        private readonly IChatService chatService;
        private readonly IUserService userService;
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
        {
            this.chatService = chatService;
            this.userService = userService;
            this.queueService = queueService;
        }

        /// <inheritdoc/>
        public string Command => "/queue";

        /// <summary>
        /// Handles incoming <see cref="Message"/> with '/queue' command.
        /// </summary>
        /// <param name="botClient"><see cref="ITelegramBotClient"/> to use.</param>
        /// <param name="message">Incoming <see cref="Message"/> to handle.</param>
        /// <returns><see cref="Message"/> which was sent in responce.</returns>
        public async Task<Message> HandleMessageAsync(ITelegramBotClient botClient, Message message)
        {
            var chat = await this.chatService.GetNewOrExistingChatAsync(message.Chat);
            var user = await this.userService.GetNewOrExistingUserAsync(message.From);
            await this.chatService.AddUserToChat(user, chat);

            var messageWords = message.Text.SplitToWords() ?? throw new ArgumentNullException("Message with null text passed to message handler.");
            if (messageWords.Length > 1)
            {
                return await this.HandleMessageWithParameters(botClient, message, messageWords, chat);
            }

            return await this.HandleMessageWithoutParameters(botClient, message, chat);
        }

        private async Task<Message> HandleMessageWithParameters(ITelegramBotClient botClient, Message message, string[] messageWords, Chat chat)
        {
            var queueName = messageWords[1];
            var queue = this.queueService.GetChatQueueByName(queueName, chat.ChatId);
            if (queue is null)
            {
                return await botClient.SendTextMessageAsync(
                    chat.ChatId,
                    $"This chat has no queue with name '{queueName}'.",
                    replyToMessageId: message.MessageId);
            }

            if (queue.Users.Count == 0)
            {
                return await botClient.SendTextMessageAsync(
                    chat.ChatId,
                    $"Queue '{queue.Name}' has no participants.");
            }

            var responceMessage = new StringBuilder($"'{queue.Name}' has these participants:{Environment.NewLine}");
            int participantPosition = 1;
            foreach (var queueParticipant in queue.Users)
            {
                responceMessage.AppendLine($"{participantPosition}) {queueParticipant.FirstName} {queueParticipant.LastName}");
                participantPosition++;
            }

            return await botClient.SendTextMessageAsync(
                chat.ChatId,
                responceMessage.ToString());
        }

        private async Task<Message> HandleMessageWithoutParameters(ITelegramBotClient botClient, Message message, Chat chat)
        {
            var chatQueues = this.queueService.GetTelegramChatQueues(chat.ChatId);
            if (chatQueues.Count() == 0)
            {
                return await botClient.SendTextMessageAsync(
                        chat.ChatId,
                        $"This chat has no queues. To create new one write '/createqueue [queue name]'.",
                        replyToMessageId: message.MessageId);
            }

            var replyMessage = new StringBuilder("This chat has these queues:\n");
            foreach (var queue in chatQueues)
            {
                replyMessage.AppendLine($"# {queue.Name}");
            }

            replyMessage.AppendLine("To get info about one of them write '/queue [queue name]'.");
            return await botClient.SendTextMessageAsync(
                        chat.ChatId,
                        replyMessage.ToString());
        }
    }
}
