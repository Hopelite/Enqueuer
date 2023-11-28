using System.Threading;
using System.Threading.Tasks;
using Enqueuer.Messaging.Core.Localization;
using Enqueuer.Messaging.Core.Types.Messages;
using Enqueuer.Persistence.Models;
using Enqueuer.Services;
using Enqueuer.Services.Extensions;
using Enqueuer.Telegram.Messages.Extensions;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using User = Enqueuer.Persistence.Models.User;

namespace Enqueuer.Telegram.Messages.MessageHandlers;

public class DequeueMessageHandler : IMessageHandler
{
    private readonly ITelegramBotClient _botClient;
    private readonly ILocalizationProvider _localizationProvider;
    private readonly IGroupService _groupService;
    private readonly IQueueService _queueService;

    public DequeueMessageHandler(ITelegramBotClient botClient, ILocalizationProvider localizationProvider, IGroupService groupService, IQueueService queueService)
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
        (var group, var user) = await _groupService.AddOrUpdateUserAndGroupAsync(messageContext.Chat, messageContext.Sender!, includeQueues: true, cancellationToken);

        if (messageContext.HasParameters())
        {
            await HandleMessageWithParameters(messageContext, group, user, cancellationToken);
            return;
        }

        await _botClient.SendTextMessageAsync(
            messageContext.Chat.Id,
            _localizationProvider.GetMessage(MessageKeys.DequeueMessageHandler.Message_DequeueCommand_PublicChat_QueueNameIsNotProvided_Message, MessageParameters.None),
            ParseMode.Html,
            replyToMessageId: messageContext.MessageId,
            cancellationToken: cancellationToken);
    }

    private async Task HandleMessageWithParameters(MessageContext messageContext, Group group, User user, CancellationToken cancellationToken)
    {
        var queueName = messageContext.Command!.GetQueueName();
        var queue = group.GetQueueByName(queueName);
        if (queue == null)
        {
            await _botClient.SendTextMessageAsync(
                group.Id,
                _localizationProvider.GetMessage(MessageKeys.DequeueMessageHandler.Message_DequeueCommand_PublicChat_QueueDoesNotExist_Message, new MessageParameters(queueName)),
                ParseMode.Html,
                replyToMessageId: messageContext.MessageId,
                cancellationToken: cancellationToken);

            return;
        }

        if (await _queueService.TryDequeueUserAsync(user, queue.Id, cancellationToken))
        {
            await _botClient.SendTextMessageAsync(
                 group.Id,
                 _localizationProvider.GetMessage(MessageKeys.DequeueMessageHandler.Message_DequeueCommand_PublicChat_SuccessfullyDequeued_Message, new MessageParameters(queueName)),
                 ParseMode.Html,
                 replyToMessageId: messageContext.MessageId,
                 cancellationToken: cancellationToken);

            return;
        }

        await _botClient.SendTextMessageAsync(
                group.Id,
                _localizationProvider.GetMessage(MessageKeys.DequeueMessageHandler.Message_DequeueCommand_PublicChat_UserDoesNotParticipate_Message, new MessageParameters(queueName)),
                ParseMode.Html,
                replyToMessageId: messageContext.MessageId,
                cancellationToken: cancellationToken);
    }
}
