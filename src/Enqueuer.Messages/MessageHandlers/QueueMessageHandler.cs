﻿using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Enqueuer.Data.TextProviders;
using Enqueuer.Messages.Extensions;
using Enqueuer.Persistence.Models;
using Enqueuer.Services;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Enqueuer.Messages.MessageHandlers;

public class QueueMessageHandler : IMessageHandler
{
    private readonly ITelegramBotClient _botClient;
    private readonly IMessageProvider _messageProvider;
    private readonly IGroupService _groupService;
    private readonly IQueueService _queueService;

    public QueueMessageHandler(ITelegramBotClient botClient, IMessageProvider messageProvider, IGroupService groupService, IQueueService queueService)
    {
        _botClient = botClient;
        _messageProvider = messageProvider;
        _groupService = groupService;
        _queueService = queueService;
    }

    public Task HandleAsync(Message message)
    {
        if (message.IsFromPrivateChat())
        {
            return _botClient.SendTextMessageAsync(
                message.Chat,
                _messageProvider.GetMessage(MessageKeys.UnsupportedCommand_PrivateChat_Message),
                ParseMode.Html);
        }

        return HandlePublicChatAsync(message);
    }

    private async Task HandlePublicChatAsync(Message message)
    {
        (var group, var _) = await _groupService.AddOrUpdateUserAndGroupAsync(message.Chat, message.From!, includeQueues: true, CancellationToken.None);

        var messageWords = message.Text!.SplitToWords();
        if (messageWords.HasParameters())
        {
            await HandleMessageWithParameters(message, messageWords, group);
            return;
        }

        await HandleMessageWithoutParameters(message, group);
    }

    private async Task HandleMessageWithParameters(Message message, string[] messageWords, Group group)
    {
        var queueName = messageWords.GetQueueName();
        var queue = await _queueService.GetQueueByNameAsync(group.Id, queueName, includeMembers: true, CancellationToken.None);
        if (queue == null)
        {
            await _botClient.SendTextMessageAsync(
                group.Id,
                _messageProvider.GetMessage(MessageKeys.QueueMessageHandler.QueueCommand_PublicChat_QueueDoesNotExist_Message, queueName),
                ParseMode.Html,
                replyToMessageId: message.MessageId);

            return;
        }

        if (queue.Members.Count == 0)
        {
            await _botClient.SendTextMessageAsync(
                group.Id,
                _messageProvider.GetMessage(MessageKeys.QueueMessageHandler.QueueCommand_PublicChat_QueueEmpty_Message, queueName),
                ParseMode.Html);

            return;
        }

        var responseMessage = BuildResponseMessageWithQueueParticipants(queue);
        await _botClient.SendTextMessageAsync(group.Id, responseMessage, ParseMode.Html);
    }

    private async Task HandleMessageWithoutParameters(Message message, Group group)
    {
        if (!group.Queues.Any())
        {
            await _botClient.SendTextMessageAsync(
                group.Id,
                _messageProvider.GetMessage(MessageKeys.QueueMessageHandler.QueueCommand_PublicChat_ListQueues_NoQueues_Message),
                ParseMode.Html,
                replyToMessageId: message.MessageId);

            return;
        }

        var replyMessage = BuildResponseMessageWithChatQueues(group.Queues);
        await _botClient.SendTextMessageAsync(
            group.Id,
            replyMessage,
            ParseMode.Html);
    }

    private string BuildResponseMessageWithChatQueues(IEnumerable<Queue> chatQueues)
    {
        var replyMessage = new StringBuilder(_messageProvider.GetMessage(MessageKeys.QueueMessageHandler.QueueCommand_PublicChat_ListQueues_Message));
        foreach (var queue in chatQueues)
        {
            replyMessage.AppendLine(_messageProvider.GetMessage(MessageKeys.QueueMessageHandler.QueueCommand_PublicChat_DisplayQueue_Message, queue.Name));
        }

        replyMessage.AppendLine(_messageProvider.GetMessage(MessageKeys.QueueMessageHandler.QueueCommand_PublicChat_ListQueues_PostScriptum_Message));
        return replyMessage.ToString();
    }

    private string BuildResponseMessageWithQueueParticipants(Queue queue)
    {
        var responseMessage = new StringBuilder(_messageProvider.GetMessage(MessageKeys.QueueMessageHandler.QueueCommand_PublicChat_ListQueueParticipants_Message, queue.Name));
        var queueParticipants = queue.Members.OrderBy(queueUser => queueUser.Position)
            .Select(queueUser => (queueUser.Position, queueUser.User));

        foreach ((var position, var user) in queueParticipants)
        {
            responseMessage.AppendLine(_messageProvider.GetMessage(MessageKeys.QueueMessageHandler.QueueCommand_PublicChat_DisplayQueueParticipant_Message, position, user.FullName));
        }

        return responseMessage.ToString();
    }
}
