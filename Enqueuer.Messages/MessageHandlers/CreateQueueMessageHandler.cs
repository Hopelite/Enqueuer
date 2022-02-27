using System.Threading.Tasks;
using Enqueuer.Data;
using Enqueuer.Data.Constants;
using Enqueuer.Data.DataSerialization;
using Enqueuer.Messages.Constants;
using Enqueuer.Persistence.Models;
using Enqueuer.Persistence.Repositories;
using Enqueuer.Services.Interfaces;
using Enqueuer.Data.Configuration;
using Enqueuer.Messages.Extensions;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Chat = Enqueuer.Persistence.Models.Chat;
using User = Enqueuer.Persistence.Models.User;

namespace Enqueuer.Messages.MessageHandlers
{
    /// <inheritdoc/>
    public class CreateQueueMessageHandler : MessageHandlerBase
    {
        private readonly IQueueService queueService;
        private readonly IRepository<Queue> queueRepository;
        private readonly IBotConfiguration botConfiguration;
        private readonly IDataSerializer dataSerializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateQueueMessageHandler"/> class.
        /// </summary>
        /// <param name="chatService">Chat service to use.</param>
        /// <param name="userService">User service to use.</param>
        /// <param name="queueService">Queue service to use.</param>
        /// <param name="queueRepository">Queue repository to use.</param>
        /// <param name="botConfiguration">Bot configuration to rely on.</param>
        /// <param name="dataSerializer"><see cref="IDataSerializer"/> to serialize with.</param>
        public CreateQueueMessageHandler(
            IChatService chatService,
            IUserService userService,
            IQueueService queueService,
            IRepository<Queue> queueRepository,
            IBotConfiguration botConfiguration,
            IDataSerializer dataSerializer)
            : base(chatService, userService)
        {
            this.queueService = queueService;
            this.queueRepository = queueRepository;
            this.botConfiguration = botConfiguration;
            this.dataSerializer = dataSerializer;
        }

        /// <inheritdoc/>
        public override string Command => "/createqueue";

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
                return await HandleMessageWithParameters(botClient, messageWords, user, chat);
            }

            return await botClient.SendTextMessageAsync(
                    message.Chat.Id,
                    "To create a new queue, please write the command this way: '<b>/createqueue</b> <i>[queue_name]</i>'.",
                    ParseMode.Html);
        }

        private async Task<Message> HandleMessageWithParameters(ITelegramBotClient botClient, string[] messageWords, User user, Chat chat)
        {
            if (ChatHasMaximalNumberOfQueues(chat))
            {
                return await botClient.SendTextMessageAsync(
                    chat.ChatId,
                    "This chat has reached its maximum number of queues. Please remove one using the '<b>/removequeue</b>' command before adding a new one.",
                    ParseMode.Html);
            }

            if (QueueHasNumberAtTheEnd(messageWords))
            {
                return await HandleMessageWithNumberAtTheEndInName(botClient, messageWords, chat);
            }

            return await this.HandleMessageWithQueueName(botClient, messageWords, user, chat);
        }

        private static async Task<Message> HandleMessageWithNumberAtTheEndInName(ITelegramBotClient botClient, string[] messageWords, Chat chat)
        {
            var responceMessage = messageWords.Length > 2
                                ? "Unable to create a queue with a number at the last position of its name. Please concat the queue name like this: '<b>Test 23</b>' => '<b>Test23</b>' or remove the number."
                                : "Unable to create a queue with only a number in its name. Please add some nice words.";
            return await botClient.SendTextMessageAsync(
                chat.ChatId,
                responceMessage,
                ParseMode.Html);
        }

        private async Task<Message> HandleMessageWithQueueName(ITelegramBotClient botClient, string[] messageWords, User user, Chat chat)
        {
            var queueName = messageWords.GetQueueName();
            if (queueName.Length > MessageHandlersConstants.MaxQueueNameLength)
            {
                return await botClient.SendTextMessageAsync(
                    chat.ChatId,
                    "This queue name is too long. Please, provide it with a name shorter than 50 symbols."
                );
            }

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
                var callbackButtonData = new CallbackData()
                {
                    Command = CallbackConstants.EnqueueMeCommand,
                    ChatId = chat.Id,
                    QueueData = new QueueData()
                    {
                        QueueId = queue.Id,
                    },
                };

                var serializedButtonData = this.dataSerializer.Serialize(callbackButtonData);
                var replyMarkup = new InlineKeyboardMarkup(InlineKeyboardButton.WithCallbackData("Enqueue me!", serializedButtonData));
                return await botClient.SendTextMessageAsync(
                    chat.ChatId,
                    $"Successfully created a new queue '<b>{queue.Name}</b>'!",
                    ParseMode.Html,
                    replyMarkup: replyMarkup);
            }

            return await botClient.SendTextMessageAsync(
                    chat.ChatId,
                    $"This chat already has a queue named '<b>{queue.Name}</b>'. Please, use some other name for this queue or delete the existing one using '<b>/removequeue</b>'.",
                    ParseMode.Html);
        }

        private static bool QueueHasNumberAtTheEnd(string[] messageWords)
        {
            return int.TryParse(messageWords[^1], out int _);
        }

        private bool ChatHasMaximalNumberOfQueues(Chat chat)
        {
            return this.chatService.GetNumberOfQueues(chat.ChatId) >= this.botConfiguration.QueuesPerChat;
        }
    }
}
