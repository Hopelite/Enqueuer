using System.Threading;
using System.Threading.Tasks;
using Enqueuer.Core.TextProviders;
using Enqueuer.Messages.Extensions;
using Enqueuer.Persistence.Constants;
using Enqueuer.Persistence.Extensions;
using Enqueuer.Persistence.Models;
using Enqueuer.Services;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using User = Enqueuer.Persistence.Models.User;

namespace Enqueuer.Messages.MessageHandlers;

public class EnqueueMessageHandler : IMessageHandler
{
    private readonly ITelegramBotClient _botClient;
    private readonly IMessageProvider _messageProvider;
    private readonly IGroupService _groupService;
    private readonly IQueueService _queueService;

    public EnqueueMessageHandler(ITelegramBotClient botClient, IMessageProvider messageProvider, IGroupService groupService, IQueueService queueService)
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
        (var group, var user) = await _groupService.AddOrUpdateUserAndGroupAsync(message.Chat, message.From!, includeQueues: true, CancellationToken.None);

        var messageWords = message.Text!.SplitToWords();
        if (!messageWords.HasParameters())
        {
            await _botClient.SendTextMessageAsync(
                message.Chat,
                _messageProvider.GetMessage(MessageKeys.EnqueueMessageHandler.EnqueueCommand_PublicChat_QueueNameIsNotProvided_Message),
                ParseMode.Html,
                replyToMessageId: message.MessageId,
                cancellationToken: cancellationToken);

            return;
        }

        await HandleMessageWithParameters(messageWords, message, user, group, cancellationToken);
    }

    private async Task HandleMessageWithParameters(string[] messageWords, Message message, User user, Group group, CancellationToken cancellationToken)
    {
        var (queueName, userPosition) = GetQueueNameAndPosition(messageWords);
        if (IsUserPositionInvalid(userPosition))
        {
            await _botClient.SendTextMessageAsync(
                group.Id,
                _messageProvider.GetMessage(MessageKeys.EnqueueMessageHandler.EnqueueCommand_PublicChat_InvalidPositionSpecified_Message),
                ParseMode.Html,
                replyToMessageId: message.MessageId,
                cancellationToken: cancellationToken);

            return;
        }

        var queue = await _queueService.GetQueueByNameAsync(group.Id, queueName, includeMembers: true, CancellationToken.None);
        if (queue == null)
        {
            await _botClient.SendTextMessageAsync(
                group.Id,
                _messageProvider.GetMessage(MessageKeys.EnqueueMessageHandler.EnqueueCommand_PublicChat_QueueDoesNotExist_Message, queueName),
                ParseMode.Html,
                replyToMessageId: message.MessageId,
                cancellationToken: cancellationToken);

            return;
        }

        if (!user.IsParticipatingIn(queue))
        {
            await HandleMessageWithUserNotParticipatingInQueue(message, user, group, queue, userPosition, cancellationToken);
            return;
        }

        await _botClient.SendTextMessageAsync(
                group.Id,
                _messageProvider.GetMessage(MessageKeys.EnqueueMessageHandler.EnqueueCommand_PublicChat_UserAlreadyParticipates_Message, queueName),
                ParseMode.Html,
                replyToMessageId: message.MessageId,
                cancellationToken: cancellationToken);
    }

    private async Task HandleMessageWithUserNotParticipatingInQueue(Message message, User user, Group chat, Queue queue, int? position, CancellationToken cancellationToken)
    {
        if (position != null && queue.IsDynamic)
        {
            await _botClient.SendTextMessageAsync(
                chat.Id,
                _messageProvider.GetMessage(MessageKeys.EnqueueMessageHandler.EnqueueCommand_PublicChat_PositionSpecified_DynamicQueue_Message, queue.Name),
                ParseMode.Html,
                replyToMessageId: message.MessageId,
                cancellationToken: cancellationToken);

            return;
        }

        if (position.HasValue)
        {
            if (await _queueService.TryEnqueueUserOnPositionAsync(user, queue.Id, position.Value, CancellationToken.None))
            {
                await _botClient.SendTextMessageAsync(
                    chat.Id,
                    _messageProvider.GetMessage(MessageKeys.EnqueueMessageHandler.EnqueueCommand_PublicChat_SuccessfullyAddedOnPosition_Message, queue.Name, position.Value),
                    ParseMode.Html,
                    replyToMessageId: message.MessageId,
                    cancellationToken: cancellationToken);

                return;
            }

            await _botClient.SendTextMessageAsync(
                    chat.Id,
                    _messageProvider.GetMessage(MessageKeys.EnqueueMessageHandler.EnqueueCommand_PublicChat_PositionIsReserved_Message, position.Value, queue.Name),
                    ParseMode.Html,
                    replyToMessageId: message.MessageId,
                    cancellationToken: cancellationToken);

            return;
        }

        var userPosition = await _queueService.AddAtFirstAvailablePosition(user, queue.Id, CancellationToken.None);
        await _botClient.SendTextMessageAsync(
            chat.Id,
            _messageProvider.GetMessage(MessageKeys.EnqueueMessageHandler.EnqueueCommand_PublicChat_SuccessfullyAddedOnPosition_Message, queue.Name, userPosition),
            ParseMode.Html,
            replyToMessageId: message.MessageId,
            cancellationToken: cancellationToken);
    }

    private static (string QueueName, int? UserPosition) GetQueueNameAndPosition(string[] messageWords)
    {
        if (int.TryParse(messageWords[^1], out int position))
        {
            return (messageWords.GetQueueNameWithoutUserPosition(), position);
        }

        return (messageWords.GetQueueName(), null);
    }

    private static bool IsUserPositionInvalid(int? userPosition)
    {
        return userPosition.HasValue && (userPosition.Value <= 0 || userPosition.Value > QueueConstants.MaxPosition);
    }
}
