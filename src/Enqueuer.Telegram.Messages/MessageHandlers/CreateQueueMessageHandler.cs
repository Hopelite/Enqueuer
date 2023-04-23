using System.Threading;
using System.Threading.Tasks;
using Enqueuer.Persistence.Models;
using Enqueuer.Services;
using Enqueuer.Services.Exceptions;
using Enqueuer.Telegram.Core.Configuration;
using Enqueuer.Telegram.Core.Localization;
using Enqueuer.Telegram.Core.Serialization;
using Enqueuer.Telegram.Messages.Extensions;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using User = Enqueuer.Persistence.Models.User;

namespace Enqueuer.Telegram.Messages.MessageHandlers;

public class CreateQueueMessageHandler : MessageHandlerWithEnqueueMeButton
{
    private readonly ITelegramBotClient _botClient;
    private readonly IGroupService _groupService;
    private readonly IQueueService _queueService;
    private readonly IBotConfiguration _botConfiguration;

    public CreateQueueMessageHandler(ITelegramBotClient botClient, ILocalizationProvider localizationProvider, IGroupService groupService, IQueueService queueService, IBotConfiguration botConfiguration, ICallbackDataSerializer dataSerializer)
        : base(localizationProvider, dataSerializer)
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
                LocalizationProvider.GetMessage(MessageKeys.Message_UnsupportedCommand_PrivateChat_Message, MessageParameters.None),
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
                LocalizationProvider.GetMessage(MessageKeys.CreateQueueMessageHandler.Message_CreateQueueCommand_PublicChat_QueueNameIsNotProvided_Message, MessageParameters.None),
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
                LocalizationProvider.GetMessage(MessageKeys.CreateQueueMessageHandler.Message_CreateQueueCommand_PublicChat_MaxNumberOfQueuesReached_Message, MessageParameters.None),
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
                LocalizationProvider.GetMessage(MessageKeys.CreateQueueMessageHandler.Message_CreateQueueCommand_PublicChat_SuccessfullyCreatedQueue_Message, new MessageParameters(response.QueueName)),
                ParseMode.Html,
                replyMarkup: new InlineKeyboardMarkup(GetEnqueueMeButton(group, response.QueueId)),
                cancellationToken: cancellationToken);
        }
        catch (QueueNameIsTooLongException)
        {
            await _botClient.SendTextMessageAsync(
                group.Id,
                LocalizationProvider.GetMessage(MessageKeys.CreateQueueMessageHandler.Message_CreateQueueCommand_PublicChat_QueueNameIsTooLong_Message, MessageParameters.None),
                ParseMode.Html,
                replyToMessageId: message.MessageId,
                cancellationToken: cancellationToken);
        }
        catch (InvalidQueueNameException)
        {
            await _botClient.SendTextMessageAsync(
                group.Id,
                LocalizationProvider.GetMessage(MessageKeys.CreateQueueMessageHandler.Message_CreateQueueCommand_PublicChat_OnlyNumberInQueueName_Message, MessageParameters.None),
                ParseMode.Html,
                replyToMessageId: message.MessageId,
                cancellationToken: cancellationToken);
        }
        catch (QueueAlreadyExistsException)
        {
            await _botClient.SendTextMessageAsync(
                group.Id,
                LocalizationProvider.GetMessage(MessageKeys.CreateQueueMessageHandler.Message_CreateQueueCommand_PublicChat_QueueAlreadyExists_Message, new MessageParameters(queueName)),
                ParseMode.Html,
                replyToMessageId: message.MessageId,
                cancellationToken: cancellationToken);
        }
        catch (InvalidMemberPositionException)
        {
            await _botClient.SendTextMessageAsync(
                group.Id,
                LocalizationProvider.GetMessage(MessageKeys.EnqueueMessageHandler.Message_EnqueueCommand_PublicChat_InvalidPositionSpecified_Message, MessageParameters.None),
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
