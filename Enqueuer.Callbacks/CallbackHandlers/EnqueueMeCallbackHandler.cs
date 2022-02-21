using System.Linq;
using System.Threading.Tasks;
using Enqueuer.Services.Interfaces;
using Enqueuer.Utilities.Extensions;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Enqueuer.Persistence.Models;
using User = Enqueuer.Persistence.Models.User;

namespace Enqueuer.Callbacks.CallbackHandlers
{
    /// <inheritdoc/>
    public class EnqueueMeCallbackHandler : ICallbackHandler
    {
        private const int QueueNameStartIndex = 1;
        private readonly IChatService chatService;
        private readonly IUserService userService;
        private readonly IQueueService queueService;
        private readonly IUserInQueueService userInQueueService;

        /// <summary>
        /// Initializes a new instance of the <see cref="EnqueueMeCallbackHandler"/> class.
        /// </summary>
        /// <param name="chatService">Chat service to use.</param>
        /// <param name="userService">User service to use.</param>
        /// <param name="queueService">Queue service to use.</param>
        /// <param name="userInQueueService">User in queue service to use.</param>
        public EnqueueMeCallbackHandler(
            IChatService chatService,
            IUserService userService,
            IQueueService queueService,
            IUserInQueueService userInQueueService)
        {
            this.chatService = chatService;
            this.userService = userService;
            this.queueService = queueService;
            this.userInQueueService = userInQueueService;
        }

        /// <inheritdoc/>
        public string Command => "/enqueueme";

        /// <inheritdoc/>
        public async Task<Message> HandleCallbackAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery)
        {
            var callbackData = callbackQuery.Data.SplitToWords();
            var chatId = callbackQuery.Message.Chat.Id;

            var queueId = int.Parse(callbackData[1]);
            var queue = this.queueService.GetQueueById(queueId);
            if (queue is null)
            {
                return await botClient.EditMessageTextAsync(
                    chatId,
                    callbackQuery.Message.MessageId,
                    $"This queue has been deleted. Please, create new one to participate in.",
                    ParseMode.Html);
            }

            return await this.HandleCallbackWithExistingQueueAsync(botClient, callbackQuery.From, callbackQuery, queue, chatId);
        }

        private async Task<Message> HandleCallbackWithExistingQueueAsync(ITelegramBotClient botClient, Telegram.Bot.Types.User telegramUser, CallbackQuery callbackQuery, Queue queue, long chatId)
        {
            var user = await this.AddUserAndChatToDBAsync(callbackQuery, chatId);
            var userInReplyMessage = $"{(telegramUser.Username is null ? telegramUser.FirstName + (telegramUser.LastName is null ? string.Empty : " " + telegramUser.LastName) : "@" + telegramUser.Username)}";
            if (!queue.Users.Any(queueUser => queueUser.UserId == telegramUser.Id))
            {
                var positionInQueue = this.userInQueueService.GetFirstAvailablePosition(queue);
                await this.userInQueueService.AddUserToQueue(user, queue, positionInQueue);
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

        private async Task<User> AddUserAndChatToDBAsync(CallbackQuery callbackQuery, long chatId)
        {
            var user = await this.userService.GetNewOrExistingUserAsync(callbackQuery.From);
            var chat = this.chatService.GetChatByChatId(chatId);
            await this.chatService.AddUserToChatIfNotAlready(user, chat);
            return user;
        }
    }
}
