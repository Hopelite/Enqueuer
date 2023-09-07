using System.Threading;
using System.Threading.Tasks;
using Enqueuer.Persistence.Constants;
using Enqueuer.Persistence.Extensions;
using Enqueuer.Persistence.Models;
using Enqueuer.Services;
using Enqueuer.Messaging.Core.Extensions;
using Enqueuer.Messaging.Core.Localization;
using Enqueuer.Telegram.Messages.Extensions;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using User = Enqueuer.Persistence.Models.User;

namespace Enqueuer.Telegram.Messages.MessageHandlers;

public class EnqueueMessageHandler : IMessageHandler
{
    private readonly ITelegramBotClient _botClient;
    private readonly ILocalizationProvider _localizationProvider;
    private readonly IGroupService _groupService;
    private readonly IQueueService _queueService;

    public EnqueueMessageHandler(ITelegramBotClient botClient, ILocalizationProvider localizationProvider, IGroupService groupService, IQueueService queueService)
    {
        _botClient = botClient;
        _localizationProvider = localizationProvider;
        _groupService = groupService;
        _queueService = queueService;
    }

    public Task HandleAsync(Message message, CancellationToken cancellationToken)
    {
        if (message.IsFromPrivateChat())
        {
            return _botClient.SendTextMessageAsync(
                message.Chat,
                _localizationProvider.GetMessage(MessageKeys.Message_UnsupportedCommand_PrivateChat_Message, MessageParameters.None),
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
                _localizationProvider.GetMessage(MessageKeys.EnqueueMessageHandler.Message_EnqueueCommand_PublicChat_QueueNameIsNotProvided_Message, MessageParameters.None),
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
                _localizationProvider.GetMessage(MessageKeys.EnqueueMessageHandler.Message_EnqueueCommand_PublicChat_InvalidPositionSpecified_Message, MessageParameters.None),
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
                _localizationProvider.GetMessage(MessageKeys.EnqueueMessageHandler.Message_EnqueueCommand_PublicChat_QueueDoesNotExist_Message, new MessageParameters(queueName)),
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
                _localizationProvider.GetMessage(MessageKeys.EnqueueMessageHandler.Message_EnqueueCommand_PublicChat_UserAlreadyParticipates_Message, new MessageParameters(queueName)),
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
                _localizationProvider.GetMessage(MessageKeys.EnqueueMessageHandler.Message_EnqueueCommand_PublicChat_PositionSpecified_DynamicQueue_Message, new MessageParameters(queue.Name)),
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
                    _localizationProvider.GetMessage(MessageKeys.EnqueueMessageHandler.Message_EnqueueCommand_PublicChat_SuccessfullyAddedOnPosition_Message, new MessageParameters(queue.Name, position.Value.ToString())),
                    ParseMode.Html,
                    replyToMessageId: message.MessageId,
                    cancellationToken: cancellationToken);

                return;
            }

            await _botClient.SendTextMessageAsync(
                    chat.Id,
                    _localizationProvider.GetMessage(MessageKeys.EnqueueMessageHandler.Message_EnqueueCommand_PublicChat_PositionIsReserved_Message, new MessageParameters(position.Value.ToString(), queue.Name)),
                    ParseMode.Html,
                    replyToMessageId: message.MessageId,
                    cancellationToken: cancellationToken);

            return;
        }

        var userPosition = await _queueService.AddAtFirstAvailablePosition(user, queue.Id, CancellationToken.None);
        await _botClient.SendTextMessageAsync(
            chat.Id,
            _localizationProvider.GetMessage(MessageKeys.EnqueueMessageHandler.Message_EnqueueCommand_PublicChat_SuccessfullyAddedOnPosition_Message, new MessageParameters(queue.Name, userPosition.ToString())),
            ParseMode.Html,
            replyToMessageId: message.MessageId,
            cancellationToken: cancellationToken);
    }

    private static (string QueueName, int? UserPosition) GetQueueNameAndPosition(string[] messageWords)
    {
        if (int.TryParse(messageWords[^1], out var position))
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
