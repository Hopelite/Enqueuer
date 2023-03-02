using System;
using System.Linq;
using System.Threading.Tasks;
using Enqueuer.Data.TextProviders;
using Enqueuer.Messages.Extensions;
using Enqueuer.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Chat = Enqueuer.Persistence.Models.Chat;
using User = Enqueuer.Persistence.Models.User;

namespace Enqueuer.Messages.MessageHandlers;

public class RemoveQueueMessageHandler : MessageHandlerBase
{
    public RemoveQueueMessageHandler(IServiceScopeFactory scopeFactory)
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
                messageProvider.GetMessage(MessageKeys.UnsupportedCommand_PrivateChat_Message),
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
                messageProvider.GetMessage(MessageKeys.RemoveQueueMessageHandler.RemoveQueueCommand_PublicChat_QueueNameIsNotProvided_Message),
                ParseMode.Html);
    }

    private async Task<Message> HandleMessageWithParameters(IServiceProvider serviceProvider, ITelegramBotClient botClient, IMessageProvider messageProvider, Message message, string[] messageWords)
    {
        var chatService = serviceProvider.GetRequiredService<IChatService>();
        var chat = await chatService.GetOrCreateChatAsync(message.Chat);

        var userService = serviceProvider.GetRequiredService<IUserService>();
        var user = await userService.GetOrCreateUserAsync(message.From);
        await chatService.AddUserToChatIfNotAlready(user, chat);

        var queueService = serviceProvider.GetRequiredService<IQueueService>();
        var queueName = messageWords.GetQueueName();

        var queue = queueService.GetChatQueueByName(queueName, chat.ChatId);
        if (queue is null)
        {
            return await botClient.SendTextMessageAsync(
                chat.ChatId,
                messageProvider.GetMessage(MessageKeys.RemoveQueueMessageHandler.RemoveQueueCommand_PublicChat_QueueDoesNotExist_Message, queueName),
                ParseMode.Html,
                replyToMessageId: message.MessageId);
        }

        if (queue.Creator.UserId != user.UserId && !await IsUserAnAdmin(botClient, chat, user))
        {
            return await botClient.SendTextMessageAsync(
                chat.ChatId,
                messageProvider.GetMessage(MessageKeys.RemoveQueueMessageHandler.RemoveQueueCommand_PublicChat_UserHasNoRightToDelete_Message, queueName),
                ParseMode.Html,
                replyToMessageId: message.MessageId);
        }

        await queueService.DeleteQueueAsync(queue);
        return await botClient.SendTextMessageAsync(
                chat.ChatId,
                messageProvider.GetMessage(MessageKeys.RemoveQueueMessageHandler.RemoveQueueCommand_PublicChat_SuccessfullyRemovedQueue_Message, queueName),
                ParseMode.Html,
                replyToMessageId: message.MessageId);
    }

    private static async Task<bool> IsUserAnAdmin(ITelegramBotClient botClient, Chat chat, User user)
    {
        var admins = await botClient.GetChatAdministratorsAsync(chat.ChatId);
        return admins.Any(admin => admin.User.Id == user.UserId);
    }
}
