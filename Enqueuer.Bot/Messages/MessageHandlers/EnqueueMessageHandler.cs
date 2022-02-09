using System;
using System.Linq;
using System.Threading.Tasks;
using Enqueuer.Bot.Extensions;
using Enqueuer.Persistence.Models;
using Enqueuer.Persistence.Repositories;
using Enqueuer.Services.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Chat = Enqueuer.Persistence.Models.Chat;
using User = Enqueuer.Persistence.Models.User;

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
        private readonly IUserInQueueService userInQueueService;
        private readonly IRepository<UserInQueue> userInQueueRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="EnqueueMessageHandler"/> class.
        /// </summary>
        /// <param name="chatService">Chat service to use.</param>
        /// <param name="userService">User service to use.</param>
        /// <param name="queueService">Queue service to use.</param>
        /// <param name="userInQueueService">User in queue service to use.</param>
        /// <param name="userInQueueRepository">User in queue repository to use.</param>
        public EnqueueMessageHandler(
            IChatService chatService,
            IUserService userService,
            IQueueService queueService,
            IUserInQueueService userInQueueService,
            IRepository<UserInQueue> userInQueueRepository)
        {
            this.chatService = chatService;
            this.userService = userService;
            this.queueService = queueService;
            this.userInQueueService = userInQueueService;
            this.userInQueueRepository = userInQueueRepository;
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
            if (message.IsPrivateChat())
            {
                return await botClient.SendUnsupportedOperationMessage(message);
            }

            var chat = await this.chatService.GetNewOrExistingChatAsync(message.Chat);
            var user = await this.userService.GetNewOrExistingUserAsync(message.From);
            await this.chatService.AddUserToChat(user, chat);

            var messageWords = message.Text.SplitToWords() ?? throw new ArgumentNullException("Message with null text passed to message handler.");
            if (messageWords.Length > 1)
            {
                return await HandleMessageWithParameters(botClient, message, messageWords, user, chat);
            }

            return await botClient.SendTextMessageAsync(
                chat.ChatId,
                $"To be enqueued, please write command this way: '<b>/enqueue</b> <i>queue_name position(optional)</i>'.",
                ParseMode.Html,
                replyToMessageId: message.MessageId);
        }

        private async Task<Message> HandleMessageWithParameters(ITelegramBotClient botClient, Message message, string[] messageWords, User user, Chat chat)
        {
            var queueNameAndPosition = GetQueueNameAndPosition(messageWords);
            if (queueNameAndPosition.UserPosition.HasValue && queueNameAndPosition.UserPosition.Value <= 0)
            {
                return await botClient.SendTextMessageAsync(
                    chat.ChatId,
                    $"Please, use positive numbers for user position.",
                    ParseMode.Html,
                    replyToMessageId: message.MessageId);
            }

            var queue = this.queueService.GetChatQueueByName(queueNameAndPosition.QueueName, chat.ChatId);
            if (queue is null)
            {
                return await botClient.SendTextMessageAsync(
                    chat.ChatId,
                    $"There is no queue with name '<b>{queueNameAndPosition.QueueName}</b>'. You can get list of chat queues using '<b>/queue</b>' command.",
                    ParseMode.Html,
                    replyToMessageId: message.MessageId);
            }

            if (!queue.Users.Any(queueUser => queueUser.UserId == user.Id))
            {
                if (queueNameAndPosition.UserPosition.HasValue)
                {
                    if (this.userInQueueService.IsPositionReserved(queue, queueNameAndPosition.UserPosition.Value))
                    {
                        return await botClient.SendTextMessageAsync(
                                chat.ChatId,
                                $"Position '<b>{queueNameAndPosition.UserPosition.Value}</b>' in queue '<b>{queue.Name}</b>' is reserved. Please, reserve other position.",
                                ParseMode.Html,
                                replyToMessageId: message.MessageId);
                    }
                }

                int userPostion = queueNameAndPosition.UserPosition ?? this.userInQueueService.GetFirstAvailablePosition(queue);
                var userInQueue = new UserInQueue()
                {
                    Position = userPostion,
                    UserId = user.Id,
                    QueueId = queue.Id,
                };

                await this.userInQueueRepository.AddAsync(userInQueue);
                return await botClient.SendTextMessageAsync(
                    chat.ChatId,
                    $"Successfully added to queue '<b>{queue.Name}</b>' on position <b>{userPostion}</b>!",
                    ParseMode.Html,
                    replyToMessageId: message.MessageId);
            }

            return await botClient.SendTextMessageAsync(
                    chat.ChatId,
                    $"You're already participating in queue '<b>{queue.Name}</b>'.",
                    ParseMode.Html,
                    replyToMessageId: message.MessageId);
        }

        private (string QueueName, int? UserPosition) GetQueueNameAndPosition(string[] messageWords)
        {
            (string QueueName, int? UserPosition) result;
            if (int.TryParse(messageWords[^1], out int position))
            {
                result.QueueName = messageWords.GetQueueNameWithoutUserPosition();
                result.UserPosition = position;
            }
            else
            {
                result.QueueName = messageWords.GetQueueName();
                result.UserPosition = null;
            }

            return result;
        }
    }
}
