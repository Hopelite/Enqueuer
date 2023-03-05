using System;
using System.Threading;
using System.Threading.Tasks;
using Enqueuer.Data.TextProviders;
using Enqueuer.Messages.Extensions;
using Enqueuer.Persistence.Extensions;
using Enqueuer.Persistence.Models;
using Enqueuer.Services;
using Enqueuer.Services.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using User = Enqueuer.Persistence.Models.User;

namespace Enqueuer.Messages.MessageHandlers;

public class EnqueueMessageHandler : MessageHandlerBase
{
    public EnqueueMessageHandler(IServiceScopeFactory scopeFactory)
        : base(scopeFactory)
    {
    }

    protected override Task HandleAsyncImplementation(IServiceProvider serviceProvider, ITelegramBotClient botClient, Message message)
    {
        var messageProvider = serviceProvider.GetRequiredService<IMessageProvider>();
        if (message.IsFromPrivateChat())
        {
            return botClient.SendTextMessageAsync(
                message.Chat,
                messageProvider.GetMessage(MessageKeys.UnsupportedCommand_PrivateChat_Message),
                ParseMode.Html);
        }

        return HandlePublicChatAsync(serviceProvider, botClient, messageProvider, message);
    }

    private async Task HandlePublicChatAsync(IServiceProvider serviceProvider, ITelegramBotClient botClient, IMessageProvider messageProvider, Message message)
    {
        var groupService = serviceProvider.GetRequiredService<IGroupService>();
        (var group, var user) = await groupService.AddOrUpdateUserAndGroupAsync(message.Chat, message.From!, includeQueues: true, CancellationToken.None);

        var messageWords = message.Text!.SplitToWords();
        if (!messageWords.HasParameters())
        {
            await botClient.SendTextMessageAsync(
                message.Chat,
                messageProvider.GetMessage(MessageKeys.EnqueueMessageHandler.EnqueueCommand_PublicChat_QueueNameIsNotProvided_Message),
                ParseMode.Html,
                replyToMessageId: message.MessageId);

            return;
        }

        await HandleMessageWithParameters(serviceProvider, botClient, messageProvider, messageWords, message, user, group);
    }

    private async Task HandleMessageWithParameters(IServiceProvider serviceProvider, ITelegramBotClient botClient, IMessageProvider messageProvider, string[] messageWords, Message message, User user, Group group)
    {
        var (queueName, userPosition) = GetQueueNameAndPosition(messageWords);
        if (IsUserPositionInvalid(userPosition))
        {
            await botClient.SendTextMessageAsync(
                group.Id,
                messageProvider.GetMessage(MessageKeys.EnqueueMessageHandler.EnqueueCommand_PublicChat_InvalidPositionSpecified_Message),
                ParseMode.Html,
                replyToMessageId: message.MessageId);

            return;
        }

        var queue = group.GetQueueByName(queueName);
        if (queue == null)
        {
            await botClient.SendTextMessageAsync(
                group.Id,
                messageProvider.GetMessage(MessageKeys.EnqueueMessageHandler.EnqueueCommand_PublicChat_QueueDoesNotExist_Message, queueName),
                ParseMode.Html,
                replyToMessageId: message.MessageId);

            return;
        }

        if (!user.IsParticipatingIn(queue))
        {
            await HandleMessageWithUserNotParticipatingInQueue(serviceProvider, botClient, messageProvider, message, user, group, queue, userPosition);
            return;
        }

        await botClient.SendTextMessageAsync(
                group.Id,
                messageProvider.GetMessage(MessageKeys.EnqueueMessageHandler.EnqueueCommand_PublicChat_UserAlreadyParticipates_Message, queueName),
                ParseMode.Html,
                replyToMessageId: message.MessageId);
    }

    private async Task HandleMessageWithUserNotParticipatingInQueue(IServiceProvider serviceProvider, ITelegramBotClient botClient, IMessageProvider messageProvider, Message message, User user, Group chat, Queue queue, int? position)
    {
        if (position != null && queue.IsDynamic)
        {
            await botClient.SendTextMessageAsync(
                chat.Id,
                messageProvider.GetMessage(MessageKeys.EnqueueMessageHandler.EnqueueCommand_PublicChat_PositionSpecified_DynamicQueue_Message, queue.Name),
                ParseMode.Html,
                replyToMessageId: message.MessageId);

            return;
        }

        var queueService = serviceProvider.GetRequiredService<IQueueService>();
        if (position.HasValue)
        {
            if (await queueService.TryEnqueueUserOnPositionAsync(user, queue.Id, position.Value, CancellationToken.None))
            {
                await botClient.SendTextMessageAsync(
                    chat.Id,
                    messageProvider.GetMessage(MessageKeys.EnqueueMessageHandler.EnqueueCommand_PublicChat_SuccessfullyAddedOnPosition_Message, queue.Name, position.Value),
                    ParseMode.Html,
                    replyToMessageId: message.MessageId);

                return;
            }

            await botClient.SendTextMessageAsync(
                    chat.Id,
                    messageProvider.GetMessage(MessageKeys.EnqueueMessageHandler.EnqueueCommand_PublicChat_PositionIsReserved_Message, position.Value, queue.Name),
                    ParseMode.Html,
                    replyToMessageId: message.MessageId);

            return;
        }

        var userPosition = await queueService.AddAtFirstAvailablePosition(user, queue.Id, CancellationToken.None);
        await botClient.SendTextMessageAsync(
            chat.Id,
            messageProvider.GetMessage(MessageKeys.EnqueueMessageHandler.EnqueueCommand_PublicChat_SuccessfullyAddedOnPosition_Message, queue.Name, userPosition),
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
