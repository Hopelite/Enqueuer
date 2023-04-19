using System.Threading;
using System.Threading.Tasks;
using Enqueuer.Core.Configuration;
using Enqueuer.Core.Serialization;
using Enqueuer.Core.TextProviders;
using Enqueuer.Messages.Extensions;
using Enqueuer.Persistence.Models;
using Enqueuer.Services;
using Enqueuer.Services.Exceptions;
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

    public CreateQueueMessageHandler(ITelegramBotClient botClient, IMessageProvider messageProvider, IGroupService groupService, IQueueService queueService, IBotConfiguration botConfiguration, ICallbackDataSerializer dataSerializer)
        : base(messageProvider, dataSerializer)
    {
        _botClient = botClient;
        _groupService = groupService;
        _queueService = queueService;
        _botConfiguration = botConfiguration;
    }

    public override Task HandleAsync(Message message, CancellationToken cancellationToken)
    {
        if (message.IsFromPrivateChat())
        {
            return _botClient.SendTextMessageAsync(
                message.Chat,
                MessageProvider.GetMessage(MessageKeys.UnsupportedCommand_PrivateChat_Message),
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
                MessageProvider.GetMessage(MessageKeys.CreateQueueMessageHandler.CreateQueueCommand_PublicChat_QueueNameIsNotProvided_Message),
                ParseMode.Html,
                replyToMessageId: message.MessageId,
                cancellationToken: cancellationToken);
    }

    private Task HandleMessageWithParameters(string[] messageWords, Message message, Group group, User user, CancellationToken cancellationToken)
    {
        if (group.Queues.Count >= _botConfiguration.QueuesPerChat)
        {
            return _botClient.SendTextMessageAsync(
                group.Id,
                MessageProvider.GetMessage(MessageKeys.CreateQueueMessageHandler.CreateQueueCommand_PublicChat_MaxNumberOfQueuesReached_Message),
                ParseMode.Html,
                replyToMessageId: message.MessageId,
                cancellationToken: cancellationToken);
        }

        return HandleMessageWithQueueName(messageWords, message, user, group, cancellationToken);
    }

    private async Task HandleMessageWithQueueName(string[] messageWords, Message message, User user, Group group, CancellationToken cancellationToken)
    {
        var queueName = messageWords.GetQueueName();
        try
        {
            var response = await _queueService.CreateQueueAsync(user.Id, group.Id, queueName, GetSpecifiedPosition(messageWords), cancellationToken);

            await _botClient.SendTextMessageAsync(
                group.Id,
                MessageProvider.GetMessage(MessageKeys.CreateQueueMessageHandler.CreateQueueCommand_PublicChat_SuccessfullyCreatedQueue_Message, response.QueueName),
                ParseMode.Html,
                replyMarkup: new InlineKeyboardMarkup(GetEnqueueMeButton(group, response.QueueId)),
                cancellationToken: cancellationToken);
        }
        catch (QueueNameIsTooLongException)
        {
            await _botClient.SendTextMessageAsync(
                group.Id,
                MessageProvider.GetMessage(MessageKeys.CreateQueueMessageHandler.CreateQueueCommand_PublicChat_QueueNameTooLong_Message),
                ParseMode.Html,
                replyToMessageId: message.MessageId,
                cancellationToken: cancellationToken);
        }
        catch (InvalidQueueNameException)
        {
            await _botClient.SendTextMessageAsync(
                group.Id,
                MessageProvider.GetMessage(MessageKeys.CreateQueueMessageHandler.CreateQueueCommand_PublicChat_OnlyNumberInQueueName_Message),
                ParseMode.Html,
                replyToMessageId: message.MessageId,
                cancellationToken: cancellationToken);
        }
        catch (QueueAlreadyExistsException)
        {
            await _botClient.SendTextMessageAsync(
                group.Id,
                MessageProvider.GetMessage(MessageKeys.CreateQueueMessageHandler.CreateQueueCommand_PublicChat_QueueAlreadyExists_Message, queueName),
                ParseMode.Html,
                replyToMessageId: message.MessageId,
                cancellationToken: cancellationToken);
        }
        catch (InvalidMemberPositionException)
        {
            await _botClient.SendTextMessageAsync(
                group.Id,
                MessageProvider.GetMessage(MessageKeys.EnqueueMessageHandler.EnqueueCommand_PublicChat_InvalidPositionSpecified_Message),
                ParseMode.Html,
                replyToMessageId: message.MessageId,
                cancellationToken: cancellationToken);
        }
    }

    private static int? GetSpecifiedPosition(string[] messageWords)
    {
        if (int.TryParse(messageWords[^1], out var positionValue))
        {
            return positionValue;
        }

        return null;
    }
}
