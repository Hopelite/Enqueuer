using System.Threading.Tasks;
using Enqueuer.Persistence.Extensions;
using Enqueuer.Persistence.Models;
using Enqueuer.Persistence.Repositories;
using Enqueuer.Services.Interfaces;
using Enqueuer.Messages.Extensions;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Chat = Enqueuer.Persistence.Models.Chat;
using User = Enqueuer.Persistence.Models.User;

namespace Enqueuer.Messages.MessageHandlers
{
    /// <inheritdoc/>
    public class EnqueueMessageHandler : MessageHandlerBase
    {
        private readonly IQueueService queueService;
        private readonly IUserInQueueService userInQueueService;
        private readonly IRepository<UserInQueue> userInQueueRepository;
        public const string PassQueueNameMessage = "To be enqueued, please write the command this way: '<b>/enqueue</b> <i>[queue_name] [position(optional)]</i>'.";
        public const string InvalidQueuePositionMessage = "Please, use positive numbers for user position.";

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
            : base(chatService, userService)
        {
            this.queueService = queueService;
            this.userInQueueService = userInQueueService;
            this.userInQueueRepository = userInQueueRepository;
        }

        /// <inheritdoc/>
        public override string Command => "/enqueue";

        /// <inheritdoc/>
        public override async Task<Message> HandleMessageAsync(ITelegramBotClient botClient, Message message)
        {
            if (message.IsPrivateChat())
            {
                return await botClient.SendUnsupportedOperationMessage(message);
            }

            var messageWords = message.Text.SplitToWords();
            if (messageWords.HasParameters())
            {
                var (user, chat) = await this.GetNewOrExistingUserAndChat(message);
                return await HandleMessageWithParameters(botClient, message, messageWords, user, chat);
            }

            return await botClient.SendTextMessageAsync(
                message.Chat.Id,
                PassQueueNameMessage,
                ParseMode.Html,
                replyToMessageId: message.MessageId);
        }

        private async Task<Message> HandleMessageWithParameters(ITelegramBotClient botClient, Message message, string[] messageWords, User user, Chat chat)
        {
            var (queueName, userPosition) = GetQueueNameAndPosition(messageWords);
            if (IsUserPositionInvalid(userPosition))
            {
                return await botClient.SendTextMessageAsync(
                    chat.ChatId,
                    InvalidQueuePositionMessage,
                    ParseMode.Html,
                    replyToMessageId: message.MessageId);
            }

            var queue = this.queueService.GetChatQueueByName(queueName, chat.ChatId);
            if (queue is null)
            {
                return await botClient.SendTextMessageAsync(
                    chat.ChatId,
                    $"There is no queue with name '<b>{queueName}</b>'. You can get list of chat queues using '<b>/queue</b>' command.",
                    ParseMode.Html,
                    replyToMessageId: message.MessageId);
            }

            if (!user.IsParticipatingIn(queue))
            {
                return await HandleMessageWithUserNotParticipatingInQueue(botClient, message, user, chat, queue, userPosition);
            }

            return await botClient.SendTextMessageAsync(
                    chat.ChatId,
                    $"You're already participating in queue '<b>{queue.Name}</b>'.",
                    ParseMode.Html,
                    replyToMessageId: message.MessageId);
        }

        private async Task<Message> HandleMessageWithUserNotParticipatingInQueue(ITelegramBotClient botClient, Message message, User user, Chat chat, Queue queue, int? position)
        {
            if (position.HasValue && this.userInQueueService.IsPositionReserved(queue, position.Value))
            {
                return await botClient.SendTextMessageAsync(
                        chat.ChatId,
                        $"Position '<b>{position.Value}</b>' in queue '<b>{queue.Name}</b>' is reserved. Please, reserve other position.",
                        ParseMode.Html,
                        replyToMessageId: message.MessageId);
            }

            int userPosition = position ?? this.userInQueueService.GetFirstAvailablePosition(queue);

            await this.userInQueueService.AddUserToQueue(user, queue, userPosition);
            return await botClient.SendTextMessageAsync(
                chat.ChatId,
                $"Successfully added to queue '<b>{queue.Name}</b>' on position <b>{userPosition}</b>!",
                ParseMode.Html,
                replyToMessageId: message.MessageId);
        }

        private static (string QueueName, int? UserPosition) GetQueueNameAndPosition(string[] messageWords)
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

        private static bool IsUserPositionInvalid(int? userPosition)
        {
            return userPosition.HasValue && userPosition.Value <= 0;
        }
    }
}
