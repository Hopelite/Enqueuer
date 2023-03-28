using System.Threading;
using System.Threading.Tasks;
using Enqueuer.Data.TextProviders;
using Enqueuer.Messages.Extensions;
using Enqueuer.Persistence.Models;
using Enqueuer.Services;
using Enqueuer.Services.Extensions;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using User = Enqueuer.Persistence.Models.User;

namespace Enqueuer.Messages.MessageHandlers;

public class DequeueMessageHandler : IMessageHandler
{
    private readonly ITelegramBotClient _botClient;
    private readonly IMessageProvider _messageProvider;
    private readonly IGroupService _groupService;
    private readonly IQueueService _queueService;

    public DequeueMessageHandler(ITelegramBotClient botClient, IMessageProvider messageProvider, IGroupService groupService, IQueueService queueService)
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
        (var group, var user) = await _groupService.AddOrUpdateUserAndGroupAsync(message.Chat, message.From!, includeQueues: true, CancellationToken.None);

        var messageWords = message.Text!.SplitToWords();
        if (messageWords.HasParameters())
        {
            await HandleMessageWithParameters(message, messageWords, group, user);
            return;
        }

        await _botClient.SendTextMessageAsync(
            message.Chat.Id,
            _messageProvider.GetMessage(MessageKeys.DequeueMessageHandler.DequeueCommand_PublicChat_QueueNameIsNotProvided_Message),
            ParseMode.Html,
            replyToMessageId: message.MessageId);
    }

    private async Task HandleMessageWithParameters(Message message, string[] messageWords, Group group, User user)
    {
        var queueName = messageWords.GetQueueName();
        var queue = group.GetQueueByName(queueName);
        if (queue == null)
        {
            await _botClient.SendTextMessageAsync(
                group.Id,
                _messageProvider.GetMessage(MessageKeys.DequeueMessageHandler.DequeueCommand_PublicChat_QueueDoesNotExist_Message, queueName),
                ParseMode.Html,
                replyToMessageId: message.MessageId);

            return;
        }

        if (await _queueService.TryDequeueUserAsync(user, queue.Id, CancellationToken.None))
        {
            await _botClient.SendTextMessageAsync(
                 group.Id,
                 _messageProvider.GetMessage(MessageKeys.DequeueMessageHandler.DequeueCommand_PublicChat_SuccessfullyDequeued_Message, queueName),
                 ParseMode.Html,
                 replyToMessageId: message.MessageId);

            return;
        }

        await _botClient.SendTextMessageAsync(
                group.Id,
                _messageProvider.GetMessage(MessageKeys.DequeueMessageHandler.DequeueCommand_PublicChat_UserDoesNotParticipate_Message, queueName),
                ParseMode.Html,
                replyToMessageId: message.MessageId);
    }
}
