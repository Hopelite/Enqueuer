using System.Linq;
using System.Threading.Tasks;
using Enqueuer.Data;
using Enqueuer.Data.Constants;
using Enqueuer.Callbacks.CallbackHandlers.BaseClasses;
using Enqueuer.Callbacks.Exceptions;
using Enqueuer.Persistence.Models;
using Enqueuer.Services.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using User = Enqueuer.Persistence.Models.User;

namespace Enqueuer.Callbacks.CallbackHandlers
{
    /// <inheritdoc/>
    public class EnqueueMeCallbackHandler : ICallbackHandler
    {
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
        public string Command => CallbackConstants.EnqueueMeCommand;

        /// <inheritdoc/>
        public async Task<Message> HandleCallbackAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, CallbackData callbackData)
        {
            if (callbackData.QueueData is not null)
            {
                var queue = this.queueService.GetQueueById(callbackData.QueueData.QueueId);
                if (queue is null)
                {
                    return await botClient.EditMessageTextAsync(
                        callbackQuery.Message.Chat,
                        callbackQuery.Message.MessageId,
                        "This queue has been deleted. Please, create a new one to participate in.",
                        ParseMode.Html);
                }

                return await this.HandleCallbackWithExistingQueueAsync(botClient, callbackQuery.From, callbackQuery, queue, callbackData);
            }

            throw new CallbackMessageHandlingException("Null queue data passed to message handler.");
        }

        private async Task<Message> HandleCallbackWithExistingQueueAsync(ITelegramBotClient botClient, Telegram.Bot.Types.User telegramUser, CallbackQuery callbackQuery, Queue queue, CallbackData callbackData)
        {
            var user = await this.AddUserAndChatToDbAsync(callbackQuery, callbackData.ChatId);
            var userInReplyMessage = GetUserName(telegramUser);
            if (DoesUserNotParticipateInQueue(queue, user))
            {
                var positionInQueue = this.userInQueueService.GetFirstAvailablePosition(queue);
                await this.userInQueueService.AddUserToQueueAsync(user, queue, positionInQueue);
                return await botClient.SendTextMessageAsync(
                    callbackQuery.Message.Chat,
                    $"<b>{userInReplyMessage}</b> successfully added to queue '<b>{queue.Name}</b>' at the '<b>{positionInQueue}</b>' position!",
                    ParseMode.Html);
            }

            return await botClient.SendTextMessageAsync(
                    callbackQuery.Message.Chat,
                    $"<b>{userInReplyMessage}</b>, you're already participating in queue '<b>{queue.Name}</b>'!",
                    ParseMode.Html);
        }

        private static bool DoesUserNotParticipateInQueue(Queue queue, User user)
        {
            return !queue.Users.Any(queueUser => queueUser.UserId == user.Id);
        }

        private static string GetUserName(Telegram.Bot.Types.User telegramUser)
        {
            return $"{(telegramUser.Username is null ? telegramUser.FirstName + (telegramUser.LastName is null ? string.Empty : " " + telegramUser.LastName) : "@" + telegramUser.Username)}";
        }

        private async Task<User> AddUserAndChatToDbAsync(CallbackQuery callbackQuery, int chatId)
        {
            var user = await this.userService.GetNewOrExistingUserAsync(callbackQuery.From);
            var chat = this.chatService.GetChatById(chatId);
            await this.chatService.AddUserToChatIfNotAlready(user, chat);
            return user;
        }
    }
}
