using System.Threading.Tasks;
using Enqueuer.Messages.Extensions;
using Enqueuer.Persistence.Models;
using Enqueuer.Persistence.Repositories;
using Enqueuer.Services.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Enqueuer.Messages.MessageHandlers
{
    /// <inheritdoc/>
    public class DynamicQueueMessageHandler : MessageHandlerBase
    {
        private readonly IQueueService queueService;
        private readonly IRepository<Queue> queueRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicQueueMessageHandler"/> class.
        /// </summary>
        /// <param name="chatService">Chat service to use.</param>
        /// <param name="userService">User service to use.</param
        /// <param name="queueService">Queue service to use.</param>
        /// <param name="queueRepository">Queue repository </param>
        public DynamicQueueMessageHandler(
            IChatService chatService,
            IUserService userService,
            IQueueService queueService,
            IRepository<Queue> queueRepository)
            : base(chatService, userService)
        {
            this.queueService = queueService;
            this.queueRepository = queueRepository;
        }

        /// <inheritdoc/>
        public override string Command => "/dynamiq";

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
                $"Please write the command this way: '/<b>dequeue</b> <i>[queue_name]</i>'.",
                ParseMode.Html,
                replyToMessageId: message.MessageId);
        }

        private async Task<Message> HandleMessageWithParameters(ITelegramBotClient botClient, Message message, string[] messageWords, Persistence.Models.User user, Persistence.Models.Chat chat)
        {
            var queueName = messageWords.GetQueueName();
            var queue = this.queueService.GetChatQueueByName(queueName, chat.ChatId);
            if (queue is null)
            {
                return await botClient.SendTextMessageAsync(
                    chat.ChatId,
                    $"There is no queue with name '<b>{queueName}</b>'. You can get list of chat queues using '<b>/queue</b>' command.",
                    ParseMode.Html,
                    replyToMessageId: message.MessageId);
            }

            if (queue.IsDynamic)
            {
                return await botClient.SendTextMessageAsync(
                    chat.ChatId,
                    $"'<b>{queueName}</b>' queue is already dynamic.",
                    ParseMode.Html,
                    replyToMessageId: message.MessageId);
            }

            await this.MakeQueueDynamic(queue);
            return await botClient.SendTextMessageAsync(
                chat.ChatId,
                $"Successfully made '<b>{queueName}</b>' queue dynamic.",
                ParseMode.Html,
                replyToMessageId: message.MessageId);
        }

        private async Task MakeQueueDynamic(Queue queue)
        {
            queue.IsDynamic = true;
            await this.queueRepository.UpdateAsync(queue);
        }
    }
}
