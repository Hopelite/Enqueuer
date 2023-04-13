using System.Linq;
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

public class RemoveQueueMessageHandler : IMessageHandler
{
    private readonly ITelegramBotClient _botClient;
    private readonly IMessageProvider _messageProvider;
    private readonly IGroupService _groupService;
    private readonly IQueueService _queueService;

    public RemoveQueueMessageHandler(ITelegramBotClient botClient, IMessageProvider messageProvider, IGroupService groupService, IQueueService queueService)
    {
        _botClient = botClient;
        _messageProvider = messageProvider;
        _groupService = groupService;
        _queueService = queueService;
    }

    public Task HandleAsync(Message message, CancellationToken cancellationToken)
    {
        if (message.IsFromPrivateChat())
        {
            return _botClient.SendTextMessageAsync(
                message.Chat,
                _messageProvider.GetMessage(MessageKeys.UnsupportedCommand_PrivateChat_Message),
                ParseMode.Html,
                cancellationToken: cancellationToken);
        }

        return HandlePublicChatAsync(message, cancellationToken);
    }

    private async Task HandlePublicChatAsync(Message message, CancellationToken cancellationToken)
    {
        (var group, var user) = await _groupService.AddOrUpdateUserAndGroupAsync(message.Chat, message.From!, includeQueues: true, cancellationToken);

        var messageWords = message.Text!.SplitToWords();
        if (messageWords.HasParameters())
        {
            await HandleMessageWithParameters(messageWords, message, group, user, cancellationToken);
            return;
        }

        await _botClient.SendTextMessageAsync(
                message.Chat.Id,
                _messageProvider.GetMessage(MessageKeys.RemoveQueueMessageHandler.RemoveQueueCommand_PublicChat_QueueNameIsNotProvided_Message),
                ParseMode.Html,
                cancellationToken: cancellationToken);
    }

    private async Task HandleMessageWithParameters(string[] messageWords, Message message, Group group, User user, CancellationToken cancellationToken)
    {
        var queueName = messageWords.GetQueueName();
        var queue = group.GetQueueByName(queueName);
        if (queue == null)
        {
            await _botClient.SendTextMessageAsync(
                group.Id,
                _messageProvider.GetMessage(MessageKeys.RemoveQueueMessageHandler.RemoveQueueCommand_PublicChat_QueueDoesNotExist_Message, queueName),
                ParseMode.Html,
                replyToMessageId: message.MessageId,
                cancellationToken: cancellationToken);

            return;
        }

        if (queue.Creator.Id != user.Id && !await IsUserAdmin(group, user, cancellationToken))
        {
            await _botClient.SendTextMessageAsync(
                group.Id,
                _messageProvider.GetMessage(MessageKeys.RemoveQueueMessageHandler.RemoveQueueCommand_PublicChat_UserHasNoRightToDelete_Message, queueName),
                ParseMode.Html,
                replyToMessageId: message.MessageId,
                cancellationToken: cancellationToken);

            return;
        }

        await _queueService.DeleteQueueAsync(queue, cancellationToken);
        await _botClient.SendTextMessageAsync(
                group.Id,
                _messageProvider.GetMessage(MessageKeys.RemoveQueueMessageHandler.RemoveQueueCommand_PublicChat_SuccessfullyRemovedQueue_Message, queueName),
                ParseMode.Html,
                replyToMessageId: message.MessageId,
                cancellationToken: cancellationToken);
    }

    private async Task<bool> IsUserAdmin(Group group, User user, CancellationToken cancellationToken)
    {
        var admins = await _botClient.GetChatAdministratorsAsync(group.Id, cancellationToken);
        return admins.Any(admin => admin.User.Id == user.Id);
    }
}
