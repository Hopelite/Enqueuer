using System.Threading;
using System.Threading.Tasks;
using Enqueuer.Messaging.Core.Extensions;
using Enqueuer.Messaging.Core.Localization;
using Enqueuer.Messaging.Core.Types.Messages;
using Enqueuer.Persistence.Constants;
using Enqueuer.Persistence.Extensions;
using Enqueuer.Persistence.Models;
using Enqueuer.Services;
using Enqueuer.Telegram.Messages.Extensions;
using Telegram.Bot;
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

    public Task HandleAsync(MessageContext messageContext, CancellationToken cancellationToken)
    {
        if (messageContext.IsFromPrivateChat())
        {
            return _botClient.SendTextMessageAsync(
                messageContext.Chat.Id,
                _localizationProvider.GetMessage(MessageKeys.Message_UnsupportedCommand_PrivateChat_Message, MessageParameters.None),
                ParseMode.Html,
                cancellationToken: cancellationToken);
        }

        return HandlePublicChatAsync(messageContext, cancellationToken);
    }

    private async Task HandlePublicChatAsync(MessageContext messageContext, CancellationToken cancellationToken)
    {
        (var group, var user) = await _groupService.AddOrUpdateUserAndGroupAsync(messageContext.Chat, messageContext.Sender!, includeQueues: true, CancellationToken.None);

        var messageWords = messageContext.Text!.SplitToWords();
        if (!messageWords.HasParameters())
        {
            await _botClient.SendTextMessageAsync(
                messageContext.Chat.Id,
                _localizationProvider.GetMessage(MessageKeys.EnqueueMessageHandler.Message_EnqueueCommand_PublicChat_QueueNameIsNotProvided_Message, MessageParameters.None),
                ParseMode.Html,
                replyToMessageId: messageContext.MessageId,
                cancellationToken: cancellationToken);

            return;
        }

        await HandleMessageWithParameters(messageWords, messageContext, user, group, cancellationToken);
    }

    private async Task HandleMessageWithParameters(string[] messageWords, MessageContext messageContext, User user, Group group, CancellationToken cancellationToken)
    {
        var (queueName, userPosition) = GetQueueNameAndPosition(messageWords);
        if (IsUserPositionInvalid(userPosition))
        {
            await _botClient.SendTextMessageAsync(
                group.Id,
                _localizationProvider.GetMessage(MessageKeys.EnqueueMessageHandler.Message_EnqueueCommand_PublicChat_InvalidPositionSpecified_Message, MessageParameters.None),
                ParseMode.Html,
                replyToMessageId: messageContext.MessageId,
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
                replyToMessageId: messageContext.MessageId,
                cancellationToken: cancellationToken);

            return;
        }

        if (!user.IsParticipatingIn(queue))
        {
            await HandleMessageWithUserNotParticipatingInQueue(messageContext, user, group, queue, userPosition, cancellationToken);
            return;
        }

        await _botClient.SendTextMessageAsync(
                group.Id,
                _localizationProvider.GetMessage(MessageKeys.EnqueueMessageHandler.Message_EnqueueCommand_PublicChat_UserAlreadyParticipates_Message, new MessageParameters(queueName)),
                ParseMode.Html,
                replyToMessageId: messageContext.MessageId,
                cancellationToken: cancellationToken);
    }

    private async Task HandleMessageWithUserNotParticipatingInQueue(MessageContext messageContext, User user, Group chat, Queue queue, int? position, CancellationToken cancellationToken)
    {
        if (position != null && queue.IsDynamic)
        {
            await _botClient.SendTextMessageAsync(
                chat.Id,
                _localizationProvider.GetMessage(MessageKeys.EnqueueMessageHandler.Message_EnqueueCommand_PublicChat_PositionSpecified_DynamicQueue_Message, new MessageParameters(queue.Name)),
                ParseMode.Html,
                replyToMessageId: messageContext.MessageId,
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
                    replyToMessageId: messageContext.MessageId,
                    cancellationToken: cancellationToken);

                return;
            }

            await _botClient.SendTextMessageAsync(
                    chat.Id,
                    _localizationProvider.GetMessage(MessageKeys.EnqueueMessageHandler.Message_EnqueueCommand_PublicChat_PositionIsReserved_Message, new MessageParameters(position.Value.ToString(), queue.Name)),
                    ParseMode.Html,
                    replyToMessageId: messageContext.MessageId,
                    cancellationToken: cancellationToken);

            return;
        }

        var userPosition = await _queueService.AddAtFirstAvailablePosition(user, queue.Id, CancellationToken.None);
        await _botClient.SendTextMessageAsync(
            chat.Id,
            _localizationProvider.GetMessage(MessageKeys.EnqueueMessageHandler.Message_EnqueueCommand_PublicChat_SuccessfullyAddedOnPosition_Message, new MessageParameters(queue.Name, userPosition.ToString())),
            ParseMode.Html,
            replyToMessageId: messageContext.MessageId,
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
