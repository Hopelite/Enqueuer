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
        private const int UserIdStartIndex = 1;
        private const int QueueNameStartIndex = 2;
        private readonly IUserService userService;
        private readonly IQueueService queueService;
        private readonly IRepository<Queue> queueRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="EnqueueMeCallbackHandler"/> class.
        /// </summary>
        /// <param name="userService">User service to use.</param>
        /// <param name="queueService">Queue service to use.</param>
        /// <param name="queueRepository">Queue repository to use.</param>
        public EnqueueMeCallbackHandler(
            IUserService userService,
            IQueueService queueService,
            IRepository<Queue> queueRepository)
        {
            this.userService = userService;
            this.queueService = queueService;
            this.queueRepository = queueRepository;
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
            var userId = long.Parse(callbackData[UserIdStartIndex]);
            var queueName = callbackData.GetQueueName(QueueNameStartIndex);
            var chatId = callbackQuery.Message.Chat.Id;

            var user =  this.userService.GetUserByUserId(userId);
            var queue = this.queueService.GetChatQueueByName(queueName, chatId);
            if (queue is null)
            {
                return await botClient.EditMessageTextAsync(
                    chatId,
                    callbackQuery.Message.MessageId,
                    $"Queue '<b>{queueName}</b>' has been deleted.",
                    ParseMode.Html);
            }

            if (!queue.Users.Any(queueUser => queueUser.UserId == user.UserId))
            {
                queue.Users.Add(user);
                await this.queueRepository.UpdateAsync(queue);

                return await botClient.EditMessageTextAsync(
                    chatId,
                    callbackQuery.Message.MessageId,
                    $"Successfully added to queue '<b>{queue.Name}</b>'!",
                    ParseMode.Html);
            }

            return await botClient.EditMessageTextAsync(
                    chatId,
                    callbackQuery.Message.MessageId,
                    $"You're already participating in queue '<b>{queue.Name}</b>'!",
                    ParseMode.Html);
        }
    }
}
