using System;
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

public class DequeueMessageHandler : MessageHandlerBase
{
    public DequeueMessageHandler(IServiceScopeFactory scopeFactory)
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
            await HandleMessageWithParameters(serviceProvider, botClient, messageProvider, message, messageWords, group, user);
            return;
        }

        await botClient.SendTextMessageAsync(
            message.Chat.Id,
            messageProvider.GetMessage(MessageKeys.DequeueMessageHandler.DequeueCommand_PublicChat_QueueNameIsNotProvided_Message),
            ParseMode.Html,
            replyToMessageId: message.MessageId);
    }

    private async Task HandleMessageWithParameters(IServiceProvider serviceProvider, ITelegramBotClient botClient, IMessageProvider messageProvider, Message message, string[] messageWords, Group group, User user)
    {
        var queueName = messageWords.GetQueueName();
        var queue = group.GetQueueByName(queueName);
        if (queue == null)
        {
            await botClient.SendTextMessageAsync(
                group.Id,
                messageProvider.GetMessage(MessageKeys.DequeueMessageHandler.DequeueCommand_PublicChat_QueueDoesNotExist_Message, queueName),
                ParseMode.Html,
                replyToMessageId: message.MessageId);

            return;
        }

        var queueService = serviceProvider.GetRequiredService<IQueueService>();
        if (await queueService.TryDequeueUserAsync(user, queue.Id, CancellationToken.None))
        {
            await botClient.SendTextMessageAsync(
                 group.Id,
                 messageProvider.GetMessage(MessageKeys.DequeueMessageHandler.DequeueCommand_PublicChat_SuccessfullyDequeued_Message, queueName),
                 ParseMode.Html,
                 replyToMessageId: message.MessageId);

            return;
        }

        await botClient.SendTextMessageAsync(
                group.Id,
                messageProvider.GetMessage(MessageKeys.DequeueMessageHandler.DequeueCommand_PublicChat_UserDoesNotParticipate_Message, queueName),
                ParseMode.Html,
                replyToMessageId: message.MessageId);
    }
}
