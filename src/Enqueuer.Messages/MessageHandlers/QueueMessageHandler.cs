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

    protected override Task HandleAsyncImplementation(IServiceProvider serviceProvider, ITelegramBotClient botClient, Message message)
    {
        var messageProvider = serviceProvider.GetRequiredService<IMessageProvider>();
        if (message.IsFromPrivateChat())
        {
            return botClient.SendTextMessageAsync(
                message.Chat,
                messageProvider.GetMessage(MessageKeys.UnsupportedCommand_PrivateChat_Message),
                ParseMode.Html);
        }

        return HandlePublicChatAsync(serviceProvider, botClient, messageProvider, message);
    }

    private async Task HandlePublicChatAsync(IServiceProvider serviceProvider, ITelegramBotClient botClient, IMessageProvider messageProvider, Message message)
    {
        var groupService = serviceProvider.GetRequiredService<IGroupService>();
        (var group, var _) = await groupService.AddOrUpdateUserAndGroupAsync(message.Chat, message.From!, includeQueues: true, CancellationToken.None);

        var messageWords = message.Text!.SplitToWords();
        if (messageWords.HasParameters())
        {
            await HandleMessageWithParameters(serviceProvider, messageProvider, botClient, message, messageWords, group);
            return;
        }

        await HandleMessageWithoutParameters(messageProvider, botClient, message, group);
    }

    private async Task HandleMessageWithParameters(IServiceProvider serviceProvider, IMessageProvider messageProvider, ITelegramBotClient botClient, Message message, string[] messageWords, Group group)
    {
        var queueName = messageWords.GetQueueName();
        var queueService = serviceProvider.GetRequiredService<IQueueService>();

        var queue = await queueService.GetQueueByNameAsync(group.Id, queueName, includeMembers: true, CancellationToken.None);
        if (queue == null)
        {
            await botClient.SendTextMessageAsync(
                group.Id,
                messageProvider.GetMessage(MessageKeys.QueueMessageHandler.QueueCommand_PublicChat_QueueDoesNotExist_Message, queueName),
                ParseMode.Html,
                replyToMessageId: message.MessageId);

            return;
        }

        if (queue.Members.Count == 0)
        {
            await botClient.SendTextMessageAsync(
                group.Id,
                messageProvider.GetMessage(MessageKeys.QueueMessageHandler.QueueCommand_PublicChat_QueueEmpty_Message, queueName),
                ParseMode.Html);

            return;
        }

        var responseMessage = BuildResponseMessageWithQueueParticipants(queue, messageProvider);
        await botClient.SendTextMessageAsync(group.Id, responseMessage, ParseMode.Html);
    }

    private async Task HandleMessageWithoutParameters(IMessageProvider messageProvider, ITelegramBotClient botClient, Message message, Group group)
    {
        if (!group.Queues.Any())
        {
            await botClient.SendTextMessageAsync(
                group.Id,
                messageProvider.GetMessage(MessageKeys.QueueMessageHandler.QueueCommand_PublicChat_ListQueues_NoQueues_Message),
                ParseMode.Html,
                replyToMessageId: message.MessageId);

            return;
        }

        var replyMessage = BuildResponseMessageWithChatQueues(group.Queues, messageProvider);
        await botClient.SendTextMessageAsync(
            group.Id,
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
