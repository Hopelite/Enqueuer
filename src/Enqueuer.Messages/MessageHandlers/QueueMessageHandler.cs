using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Enqueuer.Data.TextProviders;
using Enqueuer.Messages.Extensions;
using Enqueuer.Persistence.Models;
using Enqueuer.Services;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Enqueuer.Messages.MessageHandlers;

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
                messageProvider.GetMessage(MessageKeys.UnsupportedCommand_PrivateChat_Message),
                ParseMode.Html);
        }

        return HandlePublicChatAsync(serviceScope.ServiceProvider, botClient, messageProvider, message);
    }

    private async Task HandlePublicChatAsync(IServiceProvider serviceProvider, ITelegramBotClient botClient, IMessageProvider messageProvider, Message message)
    {
        var chatService = serviceProvider.GetRequiredService<IGroupService>();
        var chat = await chatService.GetOrStoreGroupAsync(message.Chat, CancellationToken.None);

        var userService = serviceProvider.GetRequiredService<IUserService>();
        var user = await userService.GetOrStoreUserAsync(message.From);
        await chatService.AddUserToChatIfNotAlready(user, chat);

        var messageWords = message.Text!.SplitToWords();
        if (messageWords.HasParameters())
        {
            await HandleMessageWithParameters(serviceProvider, messageProvider, botClient, message, messageWords, chat);
            return;
        }

        await HandleMessageWithoutParameters(serviceProvider, messageProvider, botClient, message, chat);
    }

    private async Task<Message> HandleMessageWithParameters(IServiceProvider serviceProvider, IMessageProvider messageProvider, ITelegramBotClient botClient, Message message, string[] messageWords, Group chat)
    {
        var queueName = messageWords.GetQueueName();
        var queueService = serviceProvider.GetRequiredService<IQueueService>();

        var queue = queueService.GetChatQueueByName(queueName, chat.Id);
        if (queue is null)
        {
            return await botClient.SendTextMessageAsync(
                chat.Id,
                messageProvider.GetMessage(MessageKeys.QueueMessageHandler.QueueCommand_PublicChat_QueueDoesNotExist_Message, queueName),
                ParseMode.Html,
                replyToMessageId: message.MessageId);
        }

        if (queue.Members.Count == 0)
        {
            return await botClient.SendTextMessageAsync(
                chat.Id,
                messageProvider.GetMessage(MessageKeys.QueueMessageHandler.QueueCommand_PublicChat_QueueEmpty_Message, queueName),
                ParseMode.Html);
        }

        var responseMessage = BuildResponseMessageWithQueueParticipants(queue, messageProvider);
        return await botClient.SendTextMessageAsync(chat.Id, responseMessage, ParseMode.Html);
    }

    private async Task<Message> HandleMessageWithoutParameters(IServiceProvider serviceProvider, IMessageProvider messageProvider, ITelegramBotClient botClient, Message message, Group chat)
    {
        var queueService = serviceProvider.GetRequiredService<IQueueService>();

        var chatQueues = queueService.GetTelegramChatQueues(chat.Id);
        if (!chatQueues.Any())
        {
            return await botClient.SendTextMessageAsync(
                    chat.Id,
                    messageProvider.GetMessage(MessageKeys.QueueMessageHandler.QueueCommand_PublicChat_ListQueues_NoQueues_Message),
                    ParseMode.Html,
                    replyToMessageId: message.MessageId);
        }

        var replyMessage = BuildResponseMessageWithChatQueues(chatQueues, messageProvider);
        return await botClient.SendTextMessageAsync(
            chat.Id,
            replyMessage,
            ParseMode.Html);
    }

    private static string BuildResponseMessageWithChatQueues(IEnumerable<Queue> chatQueues, IMessageProvider messageProvider)
    {
        var replyMessage = new StringBuilder(messageProvider.GetMessage(MessageKeys.QueueMessageHandler.QueueCommand_PublicChat_ListQueues_Message));
        foreach (var queue in chatQueues)
        {
            replyMessage.AppendLine(messageProvider.GetMessage(MessageKeys.QueueMessageHandler.QueueCommand_PublicChat_DisplayQueue_Message, queue.Name));
        }

        replyMessage.AppendLine(messageProvider.GetMessage(MessageKeys.QueueMessageHandler.QueueCommand_PublicChat_ListQueues_PostScriptum_Message));
        return replyMessage.ToString();
    }

    private static string BuildResponseMessageWithQueueParticipants(Queue queue, IMessageProvider messageProvider)
    {
        var responseMessage = new StringBuilder(messageProvider.GetMessage(MessageKeys.QueueMessageHandler.QueueCommand_PublicChat_ListQueueParticipants_Message, queue.Name));
        var queueParticipants = queue.Members.OrderBy(queueUser => queueUser.Position)
            .Select(queueUser => (queueUser.Position, queueUser.User));

        foreach ((var position, var user) in queueParticipants)
        {
            responseMessage.AppendLine(messageProvider.GetMessage(MessageKeys.QueueMessageHandler.QueueCommand_PublicChat_DisplayQueueParticipant_Message, position, user.FullName));
        }

        return responseMessage.ToString();
    }
}
