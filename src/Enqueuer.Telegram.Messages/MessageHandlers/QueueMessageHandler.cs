using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Enqueuer.Messaging.Core.Localization;
using Enqueuer.Messaging.Core.Serialization;
using Enqueuer.Messaging.Core.Types.Messages;
using Enqueuer.Persistence.Models;
using Enqueuer.Services;
using Enqueuer.Telegram.Messages.Extensions;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Enqueuer.Telegram.Messages.MessageHandlers;

public class QueueMessageHandler : MessageHandlerWithEnqueueMeButton
{
    private readonly ITelegramBotClient _botClient;
    private readonly IGroupService _groupService;
    private readonly IQueueService _queueService;

    public QueueMessageHandler(ITelegramBotClient botClient, ILocalizationProvider localizationProvider, IGroupService groupService, IQueueService queueService, ICallbackDataSerializer dataSerializer)
        : base(localizationProvider, dataSerializer)
    {
        _botClient = botClient;
        _groupService = groupService;
        _queueService = queueService;
    }

    public override Task HandleAsync(MessageContext messageContext, CancellationToken cancellationToken)
    {
        if (messageContext.IsFromPrivateChat())
        {
            return _botClient.SendTextMessageAsync(
                messageContext.Chat.Id,
                LocalizationProvider.GetMessage(MessageKeys.Message_UnsupportedCommand_PrivateChat_Message, MessageParameters.None),
                ParseMode.Html,
                cancellationToken: cancellationToken);
        }

        return HandlePublicChatAsync(messageContext, cancellationToken);
    }

    private async Task HandlePublicChatAsync(MessageContext messageContext, CancellationToken cancellationToken)
    {
        (var group, var _) = await _groupService.AddOrUpdateUserAndGroupAsync(messageContext.Chat, messageContext.Sender!, includeQueues: true, cancellationToken);

        if (messageContext.HasParameters())
        {
            await HandleMessageWithParameters(messageContext, group, cancellationToken);
            return;
        }

        await HandleMessageWithoutParameters(group, cancellationToken);
    }

    private async Task HandleMessageWithParameters(MessageContext messageContext, Group group, CancellationToken cancellationToken)
    {
        var queueName = messageContext.Command!.GetQueueName();
        var queue = await _queueService.GetQueueByNameAsync(group.Id, queueName, includeMembers: true, cancellationToken);
        if (queue == null)
        {
            await _botClient.SendTextMessageAsync(
                group.Id,
                LocalizationProvider.GetMessage(MessageKeys.QueueMessageHandler.Message_QueueCommand_PublicChat_QueueDoesNotExist_Message, new MessageParameters(queueName)),
                ParseMode.Html,
                replyToMessageId: messageContext.MessageId,
                cancellationToken: cancellationToken);

            return;
        }

        if (queue.Members.Count == 0)
        {
            await _botClient.SendTextMessageAsync(
                group.Id,
                LocalizationProvider.GetMessage(MessageKeys.QueueMessageHandler.Message_QueueCommand_PublicChat_QueueIsEmpty_Message, new MessageParameters(queueName)),
                ParseMode.Html,
                replyMarkup: new InlineKeyboardMarkup(GetEnqueueMeButton(group, queue.Id)),
                cancellationToken: cancellationToken);

            return;
        }

        var responseMessage = BuildResponseMessageWithQueueParticipants(queue);
        await _botClient.SendTextMessageAsync(group.Id, responseMessage, ParseMode.Html, cancellationToken: cancellationToken);
    }

    private async Task HandleMessageWithoutParameters(Group group, CancellationToken cancellationToken)
    {
        if (group.Queues.Count == 0)
        {
            await _botClient.SendTextMessageAsync(
                group.Id,
                LocalizationProvider.GetMessage(MessageKeys.QueueMessageHandler.Message_QueueCommand_PublicChat_ListQueues_NoQueues_Message, MessageParameters.None),
                ParseMode.Html,
                cancellationToken: cancellationToken);

            return;
        }

        var replyMessage = BuildResponseMessageWithChatQueues(group.Queues);
        await _botClient.SendTextMessageAsync(group.Id, replyMessage, ParseMode.Html, cancellationToken: cancellationToken);
    }

    private string BuildResponseMessageWithChatQueues(IEnumerable<Queue> chatQueues)
    {
        var replyMessage = new StringBuilder(LocalizationProvider.GetMessage(MessageKeys.QueueMessageHandler.Message_QueueCommand_PublicChat_ListQueues_Message, MessageParameters.None));
        replyMessage.Append(Environment.NewLine);

        foreach (var queue in chatQueues)
        {
            replyMessage.AppendLine(LocalizationProvider.GetMessage(MessageKeys.QueueMessageHandler.Message_QueueCommand_PublicChat_DisplayQueue_Message, new MessageParameters(queue.Name)));
        }

        replyMessage.AppendLine(LocalizationProvider.GetMessage(MessageKeys.QueueMessageHandler.Message_QueueCommand_PublicChat_ListQueues_PostScriptum_Message, MessageParameters.None));
        return replyMessage.ToString();
    }

    private string BuildResponseMessageWithQueueParticipants(Queue queue)
    {
        var responseMessage = new StringBuilder(LocalizationProvider.GetMessage(MessageKeys.QueueMessageHandler.Message_QueueCommand_PublicChat_ListQueueParticipants_Message, new MessageParameters(queue.Name)));
        responseMessage.Append(Environment.NewLine);

        var queueParticipants = queue.Members.OrderBy(queueUser => queueUser.Position)
            .Select(queueUser => (queueUser.Position, queueUser.User));

        foreach ((var position, var user) in queueParticipants)
        {
            responseMessage.AppendLine(LocalizationProvider.GetMessage(MessageKeys.QueueMessageHandler.Message_QueueCommand_PublicChat_DisplayQueueParticipant_Message, new MessageParameters(position.ToString(), user.FullName)));
        }

        return responseMessage.ToString();
    }
}
