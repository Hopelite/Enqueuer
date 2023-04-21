using System.Threading;
using System.Threading.Tasks;
using Enqueuer.Core.TextProviders;
using Enqueuer.Messages.Extensions;
using Enqueuer.Persistence.Models;
using Enqueuer.Services;
using Enqueuer.Services.Extensions;
using Enqueuer.Telegram.Core.Localization;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using User = Enqueuer.Persistence.Models.User;

namespace Enqueuer.Messages.MessageHandlers;

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
        (var group, var user) = await _groupService.AddOrUpdateUserAndGroupAsync(message.Chat, message.From!, includeQueues: true, cancellationToken);

        var messageWords = message.Text!.SplitToWords();
        if (messageWords.HasParameters())
        {
            await HandleMessageWithParameters(message, messageWords, group, user, cancellationToken);
            return;
        }

        await _botClient.SendTextMessageAsync(
            message.Chat.Id,
            _localizationProvider.GetMessage(MessageKeys.DequeueMessageHandler.Message_DequeueCommand_PublicChat_QueueNameIsNotProvided_Message, MessageParameters.None),
            ParseMode.Html,
            replyToMessageId: message.MessageId,
            cancellationToken: cancellationToken);
    }

    private async Task HandleMessageWithParameters(Message message, string[] messageWords, Group group, User user, CancellationToken cancellationToken)
    {
        var queueName = messageWords.GetQueueName();
        var queue = group.GetQueueByName(queueName);
        if (queue == null)
        {
            await _botClient.SendTextMessageAsync(
                group.Id,
                _localizationProvider.GetMessage(MessageKeys.DequeueMessageHandler.Message_DequeueCommand_PublicChat_QueueDoesNotExist_Message, new MessageParameters(queueName)),
                ParseMode.Html,
                replyToMessageId: message.MessageId,
                cancellationToken: cancellationToken);

            return;
        }

        if (await _queueService.TryDequeueUserAsync(user, queue.Id, cancellationToken))
        {
            await _botClient.SendTextMessageAsync(
                 group.Id,
                 _localizationProvider.GetMessage(MessageKeys.DequeueMessageHandler.Message_DequeueCommand_PublicChat_SuccessfullyDequeued_Message, new MessageParameters(queueName)),
                 ParseMode.Html,
                 replyToMessageId: message.MessageId,
                 cancellationToken: cancellationToken);

            return;
        }

        await _botClient.SendTextMessageAsync(
                group.Id,
                _localizationProvider.GetMessage(MessageKeys.DequeueMessageHandler.Message_DequeueCommand_PublicChat_UserDoesNotParticipate_Message, new MessageParameters(queueName)),
                ParseMode.Html,
                replyToMessageId: message.MessageId,
                cancellationToken: cancellationToken);
    }
}
