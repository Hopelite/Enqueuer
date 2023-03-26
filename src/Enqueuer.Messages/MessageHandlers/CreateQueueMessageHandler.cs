using System.Threading;
using System.Threading.Tasks;
using Enqueuer.Data.Configuration;
using Enqueuer.Data.DataSerialization;
using Enqueuer.Data.TextProviders;
using Enqueuer.Messages.Extensions;
using Enqueuer.Persistence.Constants;
using Enqueuer.Persistence.Models;
using Enqueuer.Services;
using Enqueuer.Services.Extensions;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using User = Enqueuer.Persistence.Models.User;

namespace Enqueuer.Messages.MessageHandlers;

public class CreateQueueMessageHandler : MessageHandlerWithEnqueueMeButton
{
    private readonly ITelegramBotClient _botClient;
    private readonly IGroupService _groupService;
    private readonly IQueueService _queueService;
    private readonly IBotConfiguration _botConfiguration;

    public CreateQueueMessageHandler(ITelegramBotClient botClient, IMessageProvider messageProvider, IGroupService groupService, IQueueService queueService, IBotConfiguration botConfiguration, IDataSerializer dataSerializer)
        : base(messageProvider, dataSerializer)
    {
        _botClient = botClient;
        _groupService = groupService;
        _queueService = queueService;
        _botConfiguration = botConfiguration;
    }

    public override Task HandleAsync(Message message)
    {
        if (message.IsFromPrivateChat())
        {
            return _botClient.SendTextMessageAsync(
                message.Chat,
                MessageProvider.GetMessage(MessageKeys.UnsupportedCommand_PrivateChat_Message),
                ParseMode.Html);
        }

        return HandlePublicChatAsync(message);
    }

    private async Task HandlePublicChatAsync(Message message)
    {
        (var group, var user) = await _groupService.AddOrUpdateUserAndGroupAsync(message.Chat, message.From!, includeQueues: true, CancellationToken.None);

        var messageWords = message.Text!.SplitToWords();
        if (messageWords.HasParameters())
        {
            await HandleMessageWithParameters(messageWords, message, group, user);
            return;
        }

        await _botClient.SendTextMessageAsync(
                message.Chat.Id,
                MessageProvider.GetMessage(MessageKeys.CreateQueueMessageHandler.CreateQueueCommand_PublicChat_QueueNameIsNotProvided_Message),
                ParseMode.Html,
                replyToMessageId: message.MessageId);
    }

    private Task HandleMessageWithParameters(string[] messageWords, Message message, Group group, User user)
    {
        if (group.Queues.Count >= _botConfiguration.QueuesPerChat)
        {
            return _botClient.SendTextMessageAsync(
                group.Id,
                MessageProvider.GetMessage(MessageKeys.CreateQueueMessageHandler.CreateQueueCommand_PublicChat_MaxNumberOfQueuesReached_Message),
                ParseMode.Html,
                replyToMessageId: message.MessageId);
        }

        if (QueueHasNumberAtTheEnd(messageWords))
        {
            return HandleMessageWithNumberAtTheEndInName(messageWords, message, group);
        }

        return HandleMessageWithQueueName(messageWords, message, user, group);
    }

    private Task HandleMessageWithNumberAtTheEndInName(string[] messageWords, Message message, Group chat)
    {
        var responseMessage = messageWords.Length > 2
                            ? MessageKeys.CreateQueueMessageHandler.CreateQueueCommand_PublicChat_NumberAtTheEndOfQueueName_Message
                            : MessageKeys.CreateQueueMessageHandler.CreateQueueCommand_PublicChat_OnlyNumberInQueueName_Message;

        return _botClient.SendTextMessageAsync(
            chat.Id,
            MessageProvider.GetMessage(responseMessage),
            ParseMode.Html,
            replyToMessageId: message.MessageId);
    }

    private async Task HandleMessageWithQueueName(string[] messageWords, Message message, User user, Group group)
    {
        var queueName = messageWords.GetQueueName();
        if (queueName.Length > QueueConstants.MaxNameLength)
        {
            await _botClient.SendTextMessageAsync(
                group.Id,
                MessageProvider.GetMessage(MessageKeys.CreateQueueMessageHandler.CreateQueueCommand_PublicChat_QueueNameTooLong_Message),
                replyToMessageId: message.MessageId);

            return;
        }

        if (group.HasQueue(queueName))
        {
            await _botClient.SendTextMessageAsync(
                group.Id,
                MessageProvider.GetMessage(MessageKeys.CreateQueueMessageHandler.CreateQueueCommand_PublicChat_QueueAlreadyExists_Message, queueName),
                ParseMode.Html,
                replyToMessageId: message.MessageId);

            return;
        }

        var queue = new Queue
        {
            Name = queueName,
            GroupId = group.Id,
            CreatorId = user.Id,
        };

        await _queueService.AddQueueAsync(queue, CancellationToken.None);
        await _botClient.SendTextMessageAsync(
            group.Id,
            MessageProvider.GetMessage(MessageKeys.CreateQueueMessageHandler.CreateQueueCommand_PublicChat_SuccessfullyCreatedQueue_Message, queue.Name),
            ParseMode.Html,
            replyMarkup: new InlineKeyboardMarkup(GetEnqueueMeButton(group, queue)));
    }

    private static bool QueueHasNumberAtTheEnd(string[] messageWords)
    {
        return int.TryParse(messageWords[^1], out int _);
    }
}
