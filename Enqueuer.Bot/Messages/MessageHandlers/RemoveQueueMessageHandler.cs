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
    /// Handles incoming <see cref="Message"/> with '/removequeue' command.
    /// </summary>
    public class RemoveQueueMessageHandler : IMessageHandler
    {
        private readonly IChatService chatService;
        private readonly IUserService userService;
        private readonly IQueueService queueService;
        private readonly IRepository<Queue> queueRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="RemoveQueueMessageHandler"/> class.
        /// </summary>
        /// <param name="chatService">Chat service to use.</param>
        /// <param name="userService">User service to use.</param>
        /// <param name="queueService">Queue service to use.</param>
        /// <param name="queueRepository">Queue repository to use.</param>
        public RemoveQueueMessageHandler(
            IChatService chatService,
            IUserService userService,
            IQueueService queueService,
            IRepository<Queue> queueRepository)
        {
            this.chatService = chatService;
            this.userService = userService;
            this.queueService = queueService;
            this.queueRepository = queueRepository;
        }

        /// <inheritdoc/>
        public string Command => "/removequeue";

        /// <summary>
        /// Handles incoming <see cref="Message"/> with '/removequeue' command.
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

            var messageWords = message.Text.SplitToWords() ?? throw new ArgumentNullException("Message with null text passed to message handler.");
            if (messageWords.Length > 1)
            {
                var chat = await this.chatService.GetNewOrExistingChatAsync(message.Chat);
                var user = await this.userService.GetNewOrExistingUserAsync(message.From);
                await this.chatService.AddUserToChat(user, chat);

                return await HandleMessageWithParameters(botClient, message, messageWords, user, chat);
            }

            return await botClient.SendTextMessageAsync(
                    message.Chat.Id,
                    "To delete queue, please write command this way: '<b>/removequeue</b> <i>queue_name</i>'.",
                    ParseMode.Html);
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

            if (queue.Creator.UserId != user.UserId)
            {
                var admins = await botClient.GetChatAdministratorsAsync(chat.ChatId);
                if (!admins.Any(admin => admin.User.Id == user.UserId))
                {
                    return await botClient.SendTextMessageAsync(
                                chat.ChatId,
                                $"Unable to delete queue '<b>{queueName}</b>'. It can be deleted only by it's creator or chat administrators.",
                                ParseMode.Html,
                                replyToMessageId: message.MessageId);
                }
            }

            await this.queueRepository.DeleteAsync(queue);
            return await botClient.SendTextMessageAsync(
                    chat.ChatId,
                    $"Successfully deleted queue '<b>{queueName}</b>'!",
                    ParseMode.Html,
                    replyToMessageId: message.MessageId);
        }
    }
}
