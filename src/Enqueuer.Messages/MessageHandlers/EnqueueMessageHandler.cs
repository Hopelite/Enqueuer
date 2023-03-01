using System;
using System.Threading.Tasks;
using Enqueuer.Data.TextProviders;
using Enqueuer.Messages.Extensions;
using Enqueuer.Persistence.Extensions;
using Enqueuer.Persistence.Models;
using Enqueuer.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Chat = Enqueuer.Persistence.Models.Chat;
using User = Enqueuer.Persistence.Models.User;

namespace Enqueuer.Messages.MessageHandlers;

public class EnqueueMessageHandler : MessageHandlerBase
{
    public EnqueueMessageHandler(IServiceScopeFactory scopeFactory)
        : base(scopeFactory)
    {
    }

    protected override Task HandleImplementationAsync(IServiceScope serviceScope, ITelegramBotClient botClient, Message message)
    {
        var messageProvider = serviceScope.ServiceProvider.GetRequiredService<IMessageProvider>();
        if (message.IsFromPrivateChat())
        {
            return botClient.SendTextMessageAsync(
                message.Chat,
                messageProvider.GetMessage(TextKeys.UnsupportedCommand_PrivateChat_Message),
                ParseMode.Html);
        }

        return HandlePublicChatAsync(serviceScope.ServiceProvider, botClient, messageProvider, message);
    }

    private async Task HandlePublicChatAsync(IServiceProvider serviceProvider, ITelegramBotClient botClient, IMessageProvider messageProvider, Message message)
    {
        var messageWords = message.Text!.SplitToWords();
        if (!messageWords.HasParameters())
        {
            await botClient.SendTextMessageAsync(
                message.Chat,
                messageProvider.GetMessage(TextKeys.EnqueueCommand_PublicChat_QueueNameIsNotProvided_Message),
                ParseMode.Html,
                replyToMessageId: message.MessageId);

            return;
        }

        var chatService = serviceProvider.GetRequiredService<IChatService>();
        var chat = await chatService.GetOrCreateChatAsync(message.Chat);

        var userService = serviceProvider.GetRequiredService<IUserService>();
        var user = await userService.GetOrCreateUserAsync(message.From);
        await chatService.AddUserToChatIfNotAlready(user, chat);

        await HandleMessageWithParameters(serviceProvider, botClient, messageProvider, message, messageWords, user, chat);
    }

    private async Task<Message> HandleMessageWithParameters(IServiceProvider serviceProvider, ITelegramBotClient botClient, IMessageProvider messageProvider, Message message, string[] messageWords, User user, Chat chat)
    {
        var (queueName, userPosition) = GetQueueNameAndPosition(messageWords);
        if (IsUserPositionInvalid(userPosition))
        {
            return await botClient.SendTextMessageAsync(
                chat.ChatId,
                messageProvider.GetMessage(TextKeys.EnqueueCommand_PublicChat_InvalidPositionSpecified_Message),
                ParseMode.Html,
                replyToMessageId: message.MessageId);
        }

        var queueService = serviceProvider.GetRequiredService<IQueueService>();
        var queue = queueService.GetChatQueueByName(queueName, chat.ChatId);
        if (queue is null)
        {
            return await botClient.SendTextMessageAsync(
                chat.ChatId,
                messageProvider.GetMessage(TextKeys.EnqueueCommand_PublicChat_GetQueue_DoesNotExist_Message, queueName),
                ParseMode.Html,
                replyToMessageId: message.MessageId);
        }

        if (!user.IsParticipatingIn(queue))
        {
            return await HandleMessageWithUserNotParticipatingInQueue(serviceProvider, botClient, messageProvider, message, user, chat, queue, userPosition);
        }

        return await botClient.SendTextMessageAsync(
                chat.ChatId,
                messageProvider.GetMessage(TextKeys.EnqueueCommand_PublicChat_UserAlreadyParticipates_Message, queueName),
                ParseMode.Html,
                replyToMessageId: message.MessageId);
    }

    private async Task<Message> HandleMessageWithUserNotParticipatingInQueue(IServiceProvider serviceProvider, ITelegramBotClient botClient, IMessageProvider messageProvider, Message message, User user, Chat chat, Queue queue, int? position)
    {
        if (position != null && queue.IsDynamic)
        {
            return await botClient.SendTextMessageAsync(
                chat.ChatId,
                messageProvider.GetMessage(TextKeys.EnqueueCommand_PublicChat_PositionSpecified_DynamicQueue_Message, queue.Name),
                ParseMode.Html,
                replyToMessageId: message.MessageId);
        }

        var userInQueueService = serviceProvider.GetRequiredService<IUserInQueueService>();
        if (position.HasValue && userInQueueService.IsPositionReserved(queue, position.Value))
        {
            return await botClient.SendTextMessageAsync(
                    chat.ChatId,
                    messageProvider.GetMessage(TextKeys.EnqueueCommand_PublicChat_PositionIsReserved_Message, position.Value, queue.Name),
                    ParseMode.Html,
                    replyToMessageId: message.MessageId);
        }

        var userPosition = position ?? userInQueueService.GetFirstAvailablePosition(queue);

        await userInQueueService.AddUserToQueueAsync(user, queue, userPosition);
        return await botClient.SendTextMessageAsync(
            chat.ChatId,
            messageProvider.GetMessage(TextKeys.EnqueueCommand_PublicChat_SuccessfullyAddedOnPosition_Message, queue.Name, userPosition),
            ParseMode.Html,
            replyToMessageId: message.MessageId);
    }

    private static (string QueueName, int? UserPosition) GetQueueNameAndPosition(string[] messageWords)
    {
        (string QueueName, int? UserPosition) result;
        if (int.TryParse(messageWords[^1], out int position))
        {
            result.QueueName = messageWords.GetQueueNameWithoutUserPosition();
            result.UserPosition = position;
        }
        else
        {
            result.QueueName = messageWords.GetQueueName();
            result.UserPosition = null;
        }

        return result;
    }

    private static bool IsUserPositionInvalid(int? userPosition)
    {
        return userPosition.HasValue && userPosition.Value <= 0;
    }
}
