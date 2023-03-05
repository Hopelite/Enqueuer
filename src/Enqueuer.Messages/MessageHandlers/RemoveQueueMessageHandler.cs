using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Enqueuer.Data.TextProviders;
using Enqueuer.Messages.Extensions;
using Enqueuer.Persistence.Models;
using Enqueuer.Services;
using Enqueuer.Services.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using User = Enqueuer.Persistence.Models.User;

namespace Enqueuer.Messages.MessageHandlers;

public class RemoveQueueMessageHandler : MessageHandlerBase
{
    public RemoveQueueMessageHandler(IServiceScopeFactory scopeFactory)
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
        if (messageWords.HasParameters())
        {
            await HandleMessageWithParameters(serviceProvider, botClient, messageProvider, messageWords, message, group, user);
            return;
        }

        await botClient.SendTextMessageAsync(
                message.Chat.Id,
                messageProvider.GetMessage(MessageKeys.RemoveQueueMessageHandler.RemoveQueueCommand_PublicChat_QueueNameIsNotProvided_Message),
                ParseMode.Html);
    }

    private async Task<Message> HandleMessageWithParameters(IServiceProvider serviceProvider, ITelegramBotClient botClient, IMessageProvider messageProvider, string[] messageWords, Message message, Group group, User user)
    {
        var queueName = messageWords.GetQueueName();
        var queue = group.GetQueueByName(queueName);
        if (queue == null)
        {
            return await botClient.SendTextMessageAsync(
                group.Id,
                messageProvider.GetMessage(MessageKeys.RemoveQueueMessageHandler.RemoveQueueCommand_PublicChat_QueueDoesNotExist_Message, queueName),
                ParseMode.Html,
                replyToMessageId: message.MessageId);
        }

        if (queue.Creator.Id != user.Id && !await IsUserAdmin(botClient, group, user))
        {
            return await botClient.SendTextMessageAsync(
                group.Id,
                messageProvider.GetMessage(MessageKeys.RemoveQueueMessageHandler.RemoveQueueCommand_PublicChat_UserHasNoRightToDelete_Message, queueName),
                ParseMode.Html,
                replyToMessageId: message.MessageId);
        }

        var queueService = serviceProvider.GetRequiredService<IQueueService>();
        await queueService.DeleteQueueAsync(queue, CancellationToken.None);
        return await botClient.SendTextMessageAsync(
                group.Id,
                messageProvider.GetMessage(MessageKeys.RemoveQueueMessageHandler.RemoveQueueCommand_PublicChat_SuccessfullyRemovedQueue_Message, queueName),
                ParseMode.Html,
                replyToMessageId: message.MessageId);
    }

    private static async Task<bool> IsUserAdmin(ITelegramBotClient botClient, Group chat, User user)
    {
        var admins = await botClient.GetChatAdministratorsAsync(chat.Id);
        return admins.Any(admin => admin.User.Id == user.Id);
    }
}
