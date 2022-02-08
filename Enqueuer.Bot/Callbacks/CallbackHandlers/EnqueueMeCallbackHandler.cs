using System.Linq;
using System.Threading.Tasks;
using Enqueuer.Bot.Extensions;
using Enqueuer.Persistence.Models;
using Enqueuer.Persistence.Repositories;
using Enqueuer.Services.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Enqueuer.Bot.Callbacks.CallbackHandlers
{
    /// <inheritdoc/>
    public class EnqueueMeCallbackHandler : ICallbackHandler
    {
        private const int QueueNameStartIndex = 1;
        private readonly IChatService chatService;
        private readonly IUserService userService;
        private readonly IQueueService queueService;
        private readonly IUserInQueueService userInQueueService;
        private readonly IRepository<UserInQueue> userInQueueRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="EnqueueMeCallbackHandler"/> class.
        /// </summary>
        /// <param name="chatService">Chat service to use.</param>
        /// <param name="userService">User service to use.</param>
        /// <param name="queueService">Queue service to use.</param>
        /// <param name="userInQueueService">User in queue service to use.</param>
        /// <param name="userInQueueRepository">User in queue repository to use.</param>
        public EnqueueMeCallbackHandler(
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
        public string Command => "/enqueueme";

        /// <summary>
        /// Handles incoming <paramref name="callbackQuery"/> with '/enqueueme' command.
        /// </summary>
        /// <param name="botClient"><see cref="ITelegramBotClient"/> to use.</param>
        /// <param name="callbackQuery">Incoming <see cref="CallbackQuery"/> to handle.</param>
        /// <returns><see cref="Message"/> which was sent in responce.</returns>
        public async Task<Message> HandleCallbackAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery)
        {
            var callbackData = callbackQuery.Data.SplitToWords();
            var queueName = callbackData.GetQueueName(QueueNameStartIndex);
            var chatId = callbackQuery.Message.Chat.Id;
            var telegramUser = callbackQuery.From;

            var user = await this.userService.GetNewOrExistingUserAsync(telegramUser);
            var chat = this.chatService.GetChatByChatId(chatId);
            await this.chatService.AddUserToChat(user, chat);

            var queue = this.queueService.GetChatQueueByName(queueName, chatId);
            if (queue is null)
            {
                return await botClient.EditMessageTextAsync(
                    chatId,
                    callbackQuery.Message.MessageId,
                    $"Queue '<b>{queueName}</b>' has been deleted. Please, create new one to participate.",
                    ParseMode.Html);
            }

            var userInReplyMessage = $"{(telegramUser.Username is null ? telegramUser.FirstName + (telegramUser.LastName is null ? string.Empty : " " + telegramUser.LastName) : "@" + telegramUser.Username)}";
            if (!queue.Users.Any(queueUser => queueUser.UserId == user.Id))
            {
                var lastPositionInQueue = this.userInQueueService.GetTotalUsersInQueue(queue);
                var userInQueue = new UserInQueue()
                {
                    Position = ++lastPositionInQueue,
                    UserId = user.Id,
                    QueueId = queue.Id,
                };

                await this.userInQueueRepository.AddAsync(userInQueue);

                return await botClient.SendTextMessageAsync(
                    chatId,
                    $"<b>{userInReplyMessage}</b> successfully added to queue '<b>{queue.Name}</b>'!",
                    ParseMode.Html);
            }

            return await botClient.SendTextMessageAsync(
                    chatId,
                    $"<b>{userInReplyMessage}</b>, you're already participating in queue '<b>{queue.Name}</b>'!",
                    ParseMode.Html);
        }
    }
}
