using System;
using System.Linq;
using System.Threading.Tasks;
using Enqueuer.Bot.Extensions;
using Enqueuer.Persistence.Models;
using Enqueuer.Persistence.Repositories;
using Enqueuer.Services.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Enqueuer.Bot.Messages.MessageHandlers
{
    /// <summary>
    /// Handles incoming <see cref="Message"/> with '/enqueue' command.
    /// </summary>
    public class EnqueueMessageHandler : IMessageHandler
    {
        private readonly IChatService chatService;
        private readonly IUserService userService;
        private readonly IQueueService queueService;
        private readonly IRepository<Queue> queueRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="EnqueueMessageHandler"/> class.
        /// </summary>
        /// <param name="chatService">Chat service to use.</param>
        /// <param name="userService">User service to use.</param>
        /// <param name="queueService">Queue service to use.</param>
        /// <param name="queueRepository">Queue repository to use.</param>
        public EnqueueMessageHandler(
            IChatService chatService,
            IUserService userService,
            IQueueService queueService,
            IRepository<Queue> queueRepository)
        {
            this.chatService = chatService;
            this.userService = userService;
            this.queueService = queueService;
            this.queueRepository = queueRepository;
        }

        /// <inheritdoc/>
        public string Command => "/enqueue";

        /// <summary>
        /// Handles incoming <see cref="Message"/> with '/enqueue' command.
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
                var queueName = messageWords[1];
                var queue = this.queueService.GetChatQueueByName(queueName, chat.ChatId);
                if (queue is null)
                {
                    return await botClient.SendTextMessageAsync(
                        chat.ChatId,
                        $"There is no queue with name '{queue.Name}'. You can get list of chat queues using '/queue' command.",
                        replyToMessageId: message.MessageId);
                }

                if (queue.Users.FirstOrDefault(queueUser => queueUser.UserId == user.UserId) is null)
                {
                    queue.Users.Add(user);
                    await this.queueRepository.UpdateAsync(queue);

                    return await botClient.SendTextMessageAsync(
                        chat.ChatId,
                        $"Successfully added to queue {queue.Name}!",
                        replyToMessageId: message.MessageId);
                }

                return await botClient.SendTextMessageAsync(
                        chat.ChatId,
                        $"You're already participating in queue '{queue.Name}'.",
                        replyToMessageId: message.MessageId);
            }

            return await botClient.SendTextMessageAsync(
                chat.ChatId,
                $"To be enqueued in queue, please write command this way: '/enqueue [queue name]'.",
                replyToMessageId: message.MessageId);
        }
    }
}
