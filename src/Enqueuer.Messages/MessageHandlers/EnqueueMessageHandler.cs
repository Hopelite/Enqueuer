using System.Threading;
using System.Threading.Tasks;
using Enqueuer.Data.TextProviders;
using Enqueuer.Messages.Extensions;
using Enqueuer.Persistence.Extensions;
using Enqueuer.Persistence.Models;
using Enqueuer.Services;
using Enqueuer.Services.Extensions;
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
        if (!messageWords.HasParameters())
        {
            await _botClient.SendTextMessageAsync(
                message.Chat,
                _messageProvider.GetMessage(MessageKeys.EnqueueMessageHandler.EnqueueCommand_PublicChat_QueueNameIsNotProvided_Message),
                ParseMode.Html,
                replyToMessageId: message.MessageId);

            return;
        }

        await HandleMessageWithParameters(messageWords, message, user, group);
    }

    private async Task HandleMessageWithParameters(string[] messageWords, Message message, User user, Group group)
    {
        var (queueName, userPosition) = GetQueueNameAndPosition(messageWords);
        if (IsUserPositionInvalid(userPosition))
        {
            await _botClient.SendTextMessageAsync(
                group.Id,
                _messageProvider.GetMessage(MessageKeys.EnqueueMessageHandler.EnqueueCommand_PublicChat_InvalidPositionSpecified_Message),
                ParseMode.Html,
                replyToMessageId: message.MessageId);

            return;
        }

        var queue = group.GetQueueByName(queueName);
        if (queue == null)
        {
            await _botClient.SendTextMessageAsync(
                group.Id,
                _messageProvider.GetMessage(MessageKeys.EnqueueMessageHandler.EnqueueCommand_PublicChat_QueueDoesNotExist_Message, queueName),
                ParseMode.Html,
                replyToMessageId: message.MessageId);

            return;
        }

        if (!user.IsParticipatingIn(queue))
        {
            await HandleMessageWithUserNotParticipatingInQueue(message, user, group, queue, userPosition);
            return;
        }

        await _botClient.SendTextMessageAsync(
                group.Id,
                _messageProvider.GetMessage(MessageKeys.EnqueueMessageHandler.EnqueueCommand_PublicChat_UserAlreadyParticipates_Message, queueName),
                ParseMode.Html,
                replyToMessageId: message.MessageId);
    }

    private async Task HandleMessageWithUserNotParticipatingInQueue(Message message, User user, Group chat, Queue queue, int? position)
    {
        if (position != null && queue.IsDynamic)
        {
            await _botClient.SendTextMessageAsync(
                chat.Id,
                _messageProvider.GetMessage(MessageKeys.EnqueueMessageHandler.EnqueueCommand_PublicChat_PositionSpecified_DynamicQueue_Message, queue.Name),
                ParseMode.Html,
                replyToMessageId: message.MessageId);

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
                    replyToMessageId: message.MessageId);

                return;
            }

            await _botClient.SendTextMessageAsync(
                    chat.Id,
                    _messageProvider.GetMessage(MessageKeys.EnqueueMessageHandler.EnqueueCommand_PublicChat_PositionIsReserved_Message, position.Value, queue.Name),
                    ParseMode.Html,
                    replyToMessageId: message.MessageId);

            return;
        }

        var userPosition = await _queueService.AddAtFirstAvailablePosition(user, queue.Id, CancellationToken.None);
        await _botClient.SendTextMessageAsync(
            chat.Id,
            _messageProvider.GetMessage(MessageKeys.EnqueueMessageHandler.EnqueueCommand_PublicChat_SuccessfullyAddedOnPosition_Message, queue.Name, userPosition),
            ParseMode.Html,
            replyToMessageId: message.MessageId);
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
        return userPosition.HasValue && userPosition.Value <= 0;
    }
}
