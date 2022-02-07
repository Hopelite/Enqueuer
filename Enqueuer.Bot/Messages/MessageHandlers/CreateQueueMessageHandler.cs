using System;
using System.Threading.Tasks;
using Enqueuer.Bot.Extensions;
using Enqueuer.Persistence.Models;
using Enqueuer.Persistence.Repositories;
using Enqueuer.Services.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Chat = Enqueuer.Persistence.Models.Chat;
using User = Enqueuer.Persistence.Models.User;

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
        private readonly IRepository<Queue> queueRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateQueueMessageHandler"/> class.
        /// </summary>
        /// <param name="chatService">Chat service to use.</param>
        /// <param name="userService">User service to use.</param>
        /// <param name="queueService">Queue service to use.</param>
        /// <param name="queueRepository">Queue repository to use.</param>
        public CreateQueueMessageHandler(
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
                return await HandleMessageWithParameters(botClient, messageWords, user, chat);
            }

            return await botClient.SendTextMessageAsync(
                    chat.ChatId,
                    "To create new queue, please write command this way: '<b>/createqueue</b> <i>[queue name]</i>'.",
                    ParseMode.Html);
        }

        private async Task<Message> HandleMessageWithParameters(ITelegramBotClient botClient, string[] messageWords, User user, Chat chat)
        {
            if (this.chatService.GetNumberOfQueues(chat.ChatId) >= 5)
            {
                return await botClient.SendTextMessageAsync(
                    chat.ChatId,
                    "This chat has maximum number of queues. Please remove one using '<b>/deletequeue</b>' command before adding new.",
                    ParseMode.Html);
            }

            var queueName = messageWords.GetQueueName();
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

                var replyMarkup = new InlineKeyboardMarkup(InlineKeyboardButton.WithCallbackData("Enqueue me!", $"/enqueueme {user.UserId} {queue.Name}"));
                return await botClient.SendTextMessageAsync(
                    chat.ChatId,
                    $"Successfully created new queue '<b>{queue.Name}</b>'!",
                    ParseMode.Html,
                    replyMarkup: replyMarkup);
            }

            return await botClient.SendTextMessageAsync(
                    chat.ChatId,
                    $"This chat already has queue with name '<b>{queue.Name}</b>'. Please, use other name for this queue or delete existing one using '<b>/deletequeue</b>'.",
                    ParseMode.Html);
        }
    }
}
