using System.Threading.Tasks;
using Enqueuer.Persistence.Extensions;
using Enqueuer.Services.Interfaces;
using Enqueuer.Utilities.Extensions;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Chat = Enqueuer.Persistence.Models.Chat;
using User = Enqueuer.Persistence.Models.User;

namespace Enqueuer.Messages.MessageHandlers
{
    /// <inheritdoc/>
    public class DequeueMessageHandler : MessageHandlerBase
    {
        private readonly IQueueService queueService;

        /// <summary>
        /// Initializes a new instance of the <see cref="DequeueMessageHandler"/> class.
        /// </summary>
        /// <param name="chatService">Chat service to use.</param>
        /// <param name="userService">User service to use.</param>
        /// <param name="queueService">Queue service to use.</param>
        public DequeueMessageHandler(
            IChatService chatService,
            IUserService userService,
            IQueueService queueService)
            : base(chatService, userService)
        {
            this.queueService = queueService;
        }

        /// <inheritdoc/>
        public override string Command => "/dequeue";

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
                var userAndChat = await this.GetNewOrExistingUserAndChat(message);
                return await HandleMessageWithParameters(botClient, message, messageWords, userAndChat.user, userAndChat.chat);
            }

            return await botClient.SendTextMessageAsync(
                message.Chat.Id,
                $"Please write the command this way: '/<b>dequeue</b> <i>[queue_name]</i>'.",
                ParseMode.Html,
                replyToMessageId: message.MessageId);
        }

        private async Task<Message> HandleMessageWithParameters(ITelegramBotClient botClient, Message message, string[] messageWords, User user, Chat chat)
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

            if (user.IsParticipatingIn(queue))
            {
                await this.queueService.RemoveUserAsync(queue, user);
                return await botClient.SendTextMessageAsync(
                    chat.ChatId,
                    $"Successfully removed from queue '<b>{queue.Name}</b>'!",
                    ParseMode.Html,
                    replyToMessageId: message.MessageId);
            }

            return await botClient.SendTextMessageAsync(
                    chat.ChatId,
                    $"You're not participating in queue '<b>{queue.Name}</b>'.",
                    ParseMode.Html,
                    replyToMessageId: message.MessageId);
        }
    }
}
