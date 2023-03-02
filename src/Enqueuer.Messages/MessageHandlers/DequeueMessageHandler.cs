using System;
using System.Threading.Tasks;
using Enqueuer.Data.TextProviders;
using Enqueuer.Messages.Extensions;
using Enqueuer.Persistence.Extensions;
using Enqueuer.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Enqueuer.Messages.MessageHandlers;

public class DequeueMessageHandler : MessageHandlerBase
{
    public DequeueMessageHandler(IServiceScopeFactory scopeFactory)
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

    private Task HandlePublicChatAsync(IServiceProvider serviceProvider, ITelegramBotClient botClient, IMessageProvider messageProvider, Message message)
    {
        var messageWords = message.Text!.SplitToWords();
        if (messageWords.HasParameters())
        {
            return HandleMessageWithParameters(serviceProvider, botClient, messageProvider, message, messageWords);
        }

        return botClient.SendTextMessageAsync(
            message.Chat.Id,
            messageProvider.GetMessage(TextKeys.DequeueCommand_PublicChat_QueueNameIsNotProvided_Message),
            ParseMode.Html,
            replyToMessageId: message.MessageId);
    }

    private async Task<Message> HandleMessageWithParameters(IServiceProvider serviceProvider, ITelegramBotClient botClient, IMessageProvider messageProvider, Message message, string[] messageWords)
    {
        var chatService = serviceProvider.GetRequiredService<IChatService>();
        var chat = await chatService.GetOrCreateChatAsync(message.Chat);

        var userService = serviceProvider.GetRequiredService<IUserService>();
        var user = await userService.GetOrCreateUserAsync(message.From);
        await chatService.AddUserToChatIfNotAlready(user, chat);

        var queueName = messageWords.GetQueueName();
        var queueService = serviceProvider.GetRequiredService<IQueueService>();

        var queue = queueService.GetChatQueueByName(queueName, chat.ChatId);
        if (queue is null)
        {
            return await botClient.SendTextMessageAsync(
                chat.ChatId,
                messageProvider.GetMessage(TextKeys.DequeueCommand_PublicChat_QueueDoesNotExist_Message, queueName),
                ParseMode.Html,
                replyToMessageId: message.MessageId);
        }

        if (user.IsParticipatingIn(queue))
        {
            await queueService.RemoveUserAsync(queue, user);
            return await botClient.SendTextMessageAsync(
                chat.ChatId,
                messageProvider.GetMessage(TextKeys.DequeueCommand_PublicChat_SuccessfullyDequeued_Message, queueName),
                ParseMode.Html,
                replyToMessageId: message.MessageId);
        }

        return await botClient.SendTextMessageAsync(
                chat.ChatId,
                messageProvider.GetMessage(TextKeys.DequeueCommand_PublicChat_UserDoesNotParticipate_Message, queueName),
                ParseMode.Html,
                replyToMessageId: message.MessageId);
    }
}
