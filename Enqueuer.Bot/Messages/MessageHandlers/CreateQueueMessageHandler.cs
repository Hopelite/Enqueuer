using System;
using System.Linq;
using System.Threading.Tasks;
using Enqueuer.Bot.Extensions;
using Enqueuer.Persistence.Models;
using Enqueuer.Persistence.Repositories;
using Enqueuer.Services.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Types;
using Chat = Enqueuer.Persistence.Models.Chat;

namespace Enqueuer.Bot.Messages.MessageHandlers
{
    /// <summary>
    /// Handles incoming <see cref="Message"/> with '/createqueue' command.
    /// </summary>
    public class CreateQueueMessageHandler : IMessageHandler
    {
        private readonly IChatService chatService;
        private readonly IUserService userService;
        private readonly IQueueService queueService;
        private readonly IRepository<Chat> chatRepository;
        private readonly IRepository<Queue> queueRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateQueueMessageHandler"/> class.
        /// </summary>
        /// <param name="chatService">Chat service to use.</param>
        /// <param name="userService">User service to use.</param>
        /// <param name="queueService">Queue service to use.</param>
        /// <param name="chatRepository">Chat repository to use.</param>
        /// <param name="queueRepository">Queue repository to use.</param>
        public CreateQueueMessageHandler(
            IChatService chatService,
            IUserService userService,
            IQueueService queueService,
            IRepository<Chat> chatRepository,
            IRepository<Queue> queueRepository)
        {
            this.chatService = chatService;
            this.userService = userService;
            this.queueService = queueService;
            this.chatRepository = chatRepository;
            this.queueRepository = queueRepository;
        }

        /// <inheritdoc/>
        public string Command => "/createqueue";

        /// <summary>
        /// Handles incoming <see cref="Message"/> with '/createqueue' command.
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
                    queue = new Queue()
                    {
                        Name = queueName,
                        ChatId = chat.Id,
                        CreatorId = user.Id,
                    };

                    await this.queueRepository.AddAsync(queue);
                    return await botClient.SendTextMessageAsync(
                        chat.ChatId,
                        $"Successfully created new queue '{queue.Name}'.");
                }

                return await botClient.SendTextMessageAsync(
                        chat.ChatId,
                        $"This chat already has queue with name '{queue.Name}'.");
            }

            return await botClient.SendTextMessageAsync(
                    chat.ChatId,
                    "To create new queue, write message this way: '/createqueue [queue name]'.");
        }
    }
}
