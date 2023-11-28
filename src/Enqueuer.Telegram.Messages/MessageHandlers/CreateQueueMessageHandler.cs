using System.Threading;
using System.Threading.Tasks;
using Enqueuer.Messaging.Core.Configuration;
using Enqueuer.Messaging.Core.Localization;
using Enqueuer.Messaging.Core.Serialization;
using Enqueuer.Messaging.Core.Types.Messages;
using Enqueuer.Persistence.Models;
using Enqueuer.Services;
using Enqueuer.Services.Exceptions;
using Enqueuer.Telegram.Messages.Extensions;
using Telegram.Bot;
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

    public override Task HandleAsync(MessageContext messageContext, CancellationToken cancellationToken)
    {
        if (messageContext.IsFromPrivateChat())
        {
            return _botClient.SendTextMessageAsync(
                messageContext.Chat.Id,
                LocalizationProvider.GetMessage(MessageKeys.Message_UnsupportedCommand_PrivateChat_Message, MessageParameters.None),
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
                LocalizationProvider.GetMessage(MessageKeys.CreateQueueMessageHandler.Message_CreateQueueCommand_PublicChat_QueueNameIsNotProvided_Message, MessageParameters.None),
                ParseMode.Html,
                replyToMessageId: messageContext.MessageId,
                cancellationToken: cancellationToken);
    }

    private Task HandleMessageWithParameters(MessageContext messageContext, Group group, User user, CancellationToken cancellationToken)
    {
        if (group.Queues.Count >= _botConfiguration.QueuesPerChat)
        {
            return _botClient.SendTextMessageAsync(
                group.Id,
                LocalizationProvider.GetMessage(MessageKeys.CreateQueueMessageHandler.Message_CreateQueueCommand_PublicChat_MaxNumberOfQueuesReached_Message, MessageParameters.None),
                ParseMode.Html,
                replyToMessageId: messageContext.MessageId,
                cancellationToken: cancellationToken);
        }

        return HandleMessageWithQueueName(messageContext, user, group, cancellationToken);
    }

    private async Task HandleMessageWithQueueName(MessageContext messageContext, User user, Group group, CancellationToken cancellationToken)
    {
        var queueName = messageContext.Command!.GetQueueName();
        try
        {
            var response = await _queueService.CreateQueueAsync(user.Id, group.Id, queueName, position: GetSpecifiedPosition(messageContext.Command!.Parameters), cancellationToken);

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
                replyToMessageId: messageContext.MessageId,
                cancellationToken: cancellationToken);
        }
        catch (InvalidQueueNameException)
        {
            await _botClient.SendTextMessageAsync(
                group.Id,
                LocalizationProvider.GetMessage(MessageKeys.CreateQueueMessageHandler.Message_CreateQueueCommand_PublicChat_OnlyNumberInQueueName_Message, MessageParameters.None),
                ParseMode.Html,
                replyToMessageId: messageContext.MessageId,
                cancellationToken: cancellationToken);
        }
        catch (QueueAlreadyExistsException)
        {
            await _botClient.SendTextMessageAsync(
                group.Id,
                LocalizationProvider.GetMessage(MessageKeys.CreateQueueMessageHandler.Message_CreateQueueCommand_PublicChat_QueueAlreadyExists_Message, new MessageParameters(queueName)),
                ParseMode.Html,
                replyToMessageId: messageContext.MessageId,
                cancellationToken: cancellationToken);
        }
        catch (InvalidMemberPositionException)
        {
            await _botClient.SendTextMessageAsync(
                group.Id,
                LocalizationProvider.GetMessage(MessageKeys.EnqueueMessageHandler.Message_EnqueueCommand_PublicChat_InvalidPositionSpecified_Message, MessageParameters.None),
                ParseMode.Html,
                replyToMessageId: messageContext.MessageId,
                cancellationToken: cancellationToken);
        }
    }

    private static int? GetSpecifiedPosition(string[] commandParameters)
    {
        if (int.TryParse(commandParameters[^1], out var positionValue))
        {
            return positionValue;
        }

        return null;
    }
}
