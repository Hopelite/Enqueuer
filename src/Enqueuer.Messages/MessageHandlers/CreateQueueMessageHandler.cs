using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Enqueuer.Data;
using Enqueuer.Data.Configuration;
using Enqueuer.Data.Constants;
using Enqueuer.Data.DataSerialization;
using Enqueuer.Data.TextProviders;
using Enqueuer.Messages.Constants;
using Enqueuer.Messages.Extensions;
using Enqueuer.Persistence.Models;
using Enqueuer.Services;
using Enqueuer.Services.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using User = Enqueuer.Persistence.Models.User;

namespace Enqueuer.Messages.MessageHandlers;

public class CreateQueueMessageHandler : MessageHandlerBase
{
    public CreateQueueMessageHandler(IServiceScopeFactory scopeFactory)
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
        (var group, var user) = await groupService.AddOrUpdateUserToGroupAsync(message.Chat, message.From!, includeQueues: true, CancellationToken.None);

        var messageWords = message.Text!.SplitToWords();
        if (messageWords.HasParameters())
        {
            await HandleMessageWithParameters(serviceProvider, botClient, messageProvider, messageWords, message, group, user);
            return;
        }

        await botClient.SendTextMessageAsync(
                message.Chat.Id,
                messageProvider.GetMessage(MessageKeys.CreateQueueMessageHandler.CreateQueueCommand_PublicChat_QueueNameIsNotProvided_Message),
                ParseMode.Html,
                replyToMessageId: message.MessageId);
    }

    private async Task<Message> HandleMessageWithParameters(IServiceProvider serviceProvider, ITelegramBotClient botClient, IMessageProvider messageProvider, string[] messageWords, Message message, Group group, User user)
    {
        var botConfiguration = serviceProvider.GetRequiredService<IBotConfiguration>();
        if (group.Queues.Count >= botConfiguration.QueuesPerChat)
        {
            return await botClient.SendTextMessageAsync(
                group.Id,
                messageProvider.GetMessage(MessageKeys.CreateQueueMessageHandler.CreateQueueCommand_PublicChat_MaxNumberOfQueuesReached_Message),
                ParseMode.Html,
                replyToMessageId: message.MessageId);
        }

        if (QueueHasNumberAtTheEnd(messageWords))
        {
            return await HandleMessageWithNumberAtTheEndInName(botClient, messageProvider, messageWords, message, group);
        }

        return await HandleMessageWithQueueName(serviceProvider, botClient, messageProvider, messageWords, message, user, group);
    }

    private Task<Message> HandleMessageWithNumberAtTheEndInName(ITelegramBotClient botClient, IMessageProvider messageProvider, string[] messageWords, Message message, Group chat)
    {
        var responseMessage = messageWords.Length > 2
                            ? MessageKeys.CreateQueueMessageHandler.CreateQueueCommand_PublicChat_NumberAtTheEndOfQueueName_Message
                            : MessageKeys.CreateQueueMessageHandler.CreateQueueCommand_PublicChat_OnlyNumberInQueueName_Message;

        return botClient.SendTextMessageAsync(
            chat.Id,
            messageProvider.GetMessage(responseMessage),
            ParseMode.Html,
            replyToMessageId: message.MessageId);
    }

    private async Task<Message> HandleMessageWithQueueName(IServiceProvider serviceProvider, ITelegramBotClient botClient, IMessageProvider messageProvider, string[] messageWords, Message message, User user, Group group)
    {
        var queueName = messageWords.GetQueueName();
        if (queueName.Length > MessageHandlersConstants.MaxQueueNameLength)
        {
            return await botClient.SendTextMessageAsync(
                group.Id,
                messageProvider.GetMessage(MessageKeys.CreateQueueMessageHandler.CreateQueueCommand_PublicChat_QueueNameTooLong_Message),
                replyToMessageId: message.MessageId);
        }

        if (group.HasQueue(queueName))
        {
            return await botClient.SendTextMessageAsync(
                group.Id,
                messageProvider.GetMessage(MessageKeys.CreateQueueMessageHandler.CreateQueueCommand_PublicChat_QueueAlreadyExists_Message, queueName),
                ParseMode.Html,
                replyToMessageId: message.MessageId);
        }

        var queue = new Queue
        {
            Name = queueName,
            GroupId = group.Id,
            CreatorId = user.Id,
        };

        var queueService = serviceProvider.GetRequiredService<IQueueService>();
        await queueService.AddAsync(queue);
        var callbackButtonData = new CallbackData()
        {
            Command = CallbackConstants.EnqueueMeCommand,
            ChatId = group.Id,
            QueueData = new QueueData()
            {
                QueueId = queue.Id,
            },
        };

        var dataSerializer = serviceProvider.GetRequiredService<IDataSerializer>();
        var serializedButtonData = dataSerializer.Serialize(callbackButtonData);
        var replyMarkup = new InlineKeyboardMarkup(InlineKeyboardButton.WithCallbackData(
            messageProvider.GetMessage(MessageKeys.CreateQueueMessageHandler.CreateQueueCommand_PublicChat_EnqueueMeButton),
            serializedButtonData));

        return await botClient.SendTextMessageAsync(
            group.Id,
            messageProvider.GetMessage(MessageKeys.CreateQueueMessageHandler.CreateQueueCommand_PublicChat_QueueNameTooLong_Message, queue.Name),
            ParseMode.Html,
            replyMarkup: replyMarkup);
    }

    private static bool QueueHasNumberAtTheEnd(string[] messageWords)
    {
        return int.TryParse(messageWords[^1], out int _);
    }
}
