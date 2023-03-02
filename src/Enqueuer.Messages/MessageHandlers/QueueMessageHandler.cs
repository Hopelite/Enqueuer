using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Enqueuer.Data.TextProviders;
using Enqueuer.Messages.Extensions;
using Enqueuer.Persistence.Models;
using Enqueuer.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Chat = Enqueuer.Persistence.Models.Chat;

namespace Enqueuer.Messages.MessageHandlers
{
    public class QueueMessageHandler : MessageHandlerBase
    {
        public QueueMessageHandler(IServiceScopeFactory scopeFactory)
            : base(scopeFactory)
        {
        }

        protected override Task HandleImplementationAsync(IServiceScope serviceScope, ITelegramBotClient botClient, Message message)
        {
            var messageProvider = serviceScope.ServiceProvider.GetRequiredService<IMessageProvider>();
            if (message.IsFromPrivateChat())
            {
                return botClient.SendTextMessageAsync(
                    message.Chat,
                    messageProvider.GetMessage(TextKeys.UnsupportedCommand_PrivateChat_Message),
                    ParseMode.Html);
            }

            return HandlePublicChatAsync(serviceScope.ServiceProvider, botClient, messageProvider, message);
        }

        private async Task HandlePublicChatAsync(IServiceProvider serviceProvider, ITelegramBotClient botClient, IMessageProvider messageProvider, Message message)
        {
            var chatService = serviceProvider.GetRequiredService<IChatService>();
            var chat = await chatService.GetOrCreateChatAsync(message.Chat);

            var userService = serviceProvider.GetRequiredService<IUserService>();
            var user = await userService.GetOrCreateUserAsync(message.From);
            await chatService.AddUserToChatIfNotAlready(user, chat);

            var messageWords = message.Text!.SplitToWords();
            if (messageWords.HasParameters())
            {
                await HandleMessageWithParameters(serviceProvider, messageProvider, botClient, message, messageWords, chat);
                return;
            }

            await HandleMessageWithoutParameters(serviceProvider, messageProvider, botClient, message, chat);
        }

        private async Task<Message> HandleMessageWithParameters(IServiceProvider serviceProvider, IMessageProvider messageProvider, ITelegramBotClient botClient, Message message, string[] messageWords, Chat chat)
        {
            var queueName = messageWords.GetQueueName();
            var queueService = serviceProvider.GetRequiredService<IQueueService>();

            var queue = queueService.GetChatQueueByName(queueName, chat.ChatId);
            if (queue is null)
            {
                return await botClient.SendTextMessageAsync(
                    chat.ChatId,
                    messageProvider.GetMessage(TextKeys.QueueCommand_PublicChat_QueueDoesNotExist_Message, queueName),
                    ParseMode.Html,
                    replyToMessageId: message.MessageId);
            }

            if (queue.Users.Count == 0)
            {
                return await botClient.SendTextMessageAsync(
                    chat.ChatId,
                    messageProvider.GetMessage(TextKeys.QueueCommand_PublicChat_QueueEmpty_Message, queueName),
                    ParseMode.Html);
            }

            var responseMessage = BuildResponseMessageWithQueueParticipants(queue, messageProvider);
            return await botClient.SendTextMessageAsync(chat.ChatId, responseMessage, ParseMode.Html);
        }

        private async Task<Message> HandleMessageWithoutParameters(IServiceProvider serviceProvider, IMessageProvider messageProvider, ITelegramBotClient botClient, Message message, Chat chat)
        {
            var queueService = serviceProvider.GetRequiredService<IQueueService>();

            var chatQueues = queueService.GetTelegramChatQueues(chat.ChatId);
            if (!chatQueues.Any())
            {
                return await botClient.SendTextMessageAsync(
                        chat.ChatId,
                        messageProvider.GetMessage(TextKeys.QueueCommand_PublicChat_ListQueues_NoQueues_Message),
                        ParseMode.Html,
                        replyToMessageId: message.MessageId);
            }

            var replyMessage = BuildResponseMessageWithChatQueues(chatQueues, messageProvider);
            return await botClient.SendTextMessageAsync(
                chat.ChatId,
                replyMessage,
                ParseMode.Html);
        }

        private static string BuildResponseMessageWithChatQueues(IEnumerable<Queue> chatQueues, IMessageProvider messageProvider)
        {
            var replyMessage = new StringBuilder(messageProvider.GetMessage(TextKeys.QueueCommand_PublicChat_ListQueues_Message));
            foreach (var queue in chatQueues)
            {
                replyMessage.AppendLine(messageProvider.GetMessage(TextKeys.QueueCommand_PublicChat_DisplayQueue_Message, queue.Name));
            }

            replyMessage.AppendLine(messageProvider.GetMessage(TextKeys.QueueCommand_PublicChat_ListQueues_PostScriptum_Message));
            return replyMessage.ToString();
        }

        private static string BuildResponseMessageWithQueueParticipants(Queue queue, IMessageProvider messageProvider)
        {
            var responseMessage = new StringBuilder(messageProvider.GetMessage(TextKeys.QueueCommand_PublicChat_ListQueueParticipants_Message, queue.Name));
            var queueParticipants = queue.Users.OrderBy(queueUser => queueUser.Position)
                .Select(queueUser => (queueUser.Position, queueUser.User));

            foreach ((var position, var user) in queueParticipants)
            {
                responseMessage.AppendLine(messageProvider.GetMessage(TextKeys.QueueCommand_PublicChat_DisplayQueueParticipant_Message, position, user.FullName));
            }

            return responseMessage.ToString();
        }
    }
}
