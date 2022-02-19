using System.Linq;
using System.Threading.Tasks;
using Enqueuer.Services.Interfaces;
using Enqueuer.Utilities.Extensions;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Enqueuer.Persistence.Models;

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
            var queueName = callbackData.GetQueueName(QueueNameStartIndex);
            var chatId = callbackQuery.Message.Chat.Id;

            await this.AddUserAndChatToDBAsync(callbackQuery, chatId);
            var queue = this.queueService.GetChatQueueByName(queueName, chatId);
            if (queue is null)
            {
                return await botClient.EditMessageTextAsync(
                    chatId,
                    callbackQuery.Message.MessageId,
                    $"Queue '<b>{queueName}</b>' has been deleted. Please, create new one to participate in.",
                    ParseMode.Html);
            }

            return await this.HandleCallbackWithExistingQueueAsync(botClient, callbackQuery.From, queue, chatId);
        }

        private async Task<Message> HandleCallbackWithExistingQueueAsync(ITelegramBotClient botClient, Telegram.Bot.Types.User user, Queue queue, long chatId)
        {
            var userInReplyMessage = $"{(user.Username is null ? user.FirstName + (user.LastName is null ? string.Empty : " " + user.LastName) : "@" + user.Username)}";
            if (!queue.Users.Any(queueUser => queueUser.UserId == user.Id))
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

        private async Task AddUserAndChatToDBAsync(CallbackQuery callbackQuery, long chatId)
        {
            var user = await this.userService.GetNewOrExistingUserAsync(callbackQuery.From);
            var chat = this.chatService.GetChatByChatId(chatId);
            await this.chatService.AddUserToChatIfNotAlready(user, chat);
        }
    }
}
