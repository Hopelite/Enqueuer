using System;
using System.Threading.Tasks;
using Enqueuer.Data;
using Enqueuer.Data.Configuration;
using Enqueuer.Data.Constants;
using Enqueuer.Data.DataSerialization;
using Enqueuer.Data.TextProviders;
using Enqueuer.Messages.Constants;
using Enqueuer.Messages.Extensions;
using Enqueuer.Persistence.Models;
using Enqueuer.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Chat = Enqueuer.Persistence.Models.Chat;
using User = Enqueuer.Persistence.Models.User;

namespace Enqueuer.Messages.MessageHandlers;

public class CreateQueueMessageHandler : MessageHandlerBase
{
    public CreateQueueMessageHandler(IServiceScopeFactory scopeFactory)
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
            return HandleMessageWithParameters(serviceProvider, botClient, messageProvider, messageWords, message);
        }

        return botClient.SendTextMessageAsync(
                message.Chat.Id,
                messageProvider.GetMessage(MessageKeys.CreateQueueMessageHandler.CreateQueueCommand_PublicChat_QueueNameIsNotProvided_Message),
                ParseMode.Html);
    }

    private async Task<Message> HandleMessageWithParameters(IServiceProvider serviceProvider, ITelegramBotClient botClient, IMessageProvider messageProvider, string[] messageWords, Message message)
    {
        var chatService = serviceProvider.GetRequiredService<IChatService>();
        var chat = await chatService.GetOrCreateChatAsync(message.Chat);

        var userService = serviceProvider.GetRequiredService<IUserService>();
        var user = await userService.GetOrCreateUserAsync(message.From);
        await chatService.AddUserToChatIfNotAlready(user, chat);

        var botConfiguration = serviceProvider.GetRequiredService<IBotConfiguration>();
        if (ChatHasMaximalNumberOfQueues(chatService, chat, botConfiguration))
        {
            return await botClient.SendTextMessageAsync(
                chat.ChatId,
                messageProvider.GetMessage(MessageKeys.CreateQueueMessageHandler.CreateQueueCommand_PublicChat_MaxNumberOfQueuesReached_Message),
                ParseMode.Html);
        }

        if (QueueHasNumberAtTheEnd(messageWords))
        {
            return await HandleMessageWithNumberAtTheEndInName(botClient, messageProvider, messageWords, chat);
        }

        return await HandleMessageWithQueueName(serviceProvider, botClient, messageProvider, messageWords, user, chat);
    }

    private static Task<Message> HandleMessageWithNumberAtTheEndInName(ITelegramBotClient botClient, IMessageProvider messageProvider, string[] messageWords, Chat chat)
    {
        var responseMessage = messageWords.Length > 2
                            ? MessageKeys.CreateQueueMessageHandler.CreateQueueCommand_PublicChat_NumberAtTheEndOfQueueName_Message
                            : MessageKeys.CreateQueueMessageHandler.CreateQueueCommand_PublicChat_OnlyNumberInQueueName_Message;
        return botClient.SendTextMessageAsync(
            chat.ChatId,
            messageProvider.GetMessage(responseMessage),
            ParseMode.Html);
    }

    private async Task<Message> HandleMessageWithQueueName(IServiceProvider serviceProvider, ITelegramBotClient botClient, IMessageProvider messageProvider, string[] messageWords, User user, Chat chat)
    {
        var queueName = messageWords.GetQueueName();
        if (queueName.Length > MessageHandlersConstants.MaxQueueNameLength)
        {
            return await botClient.SendTextMessageAsync(
                chat.ChatId,
                messageProvider.GetMessage(MessageKeys.CreateQueueMessageHandler.CreateQueueCommand_PublicChat_QueueNameTooLong_Message));
        }

        var queueService = serviceProvider.GetRequiredService<IQueueService>();
        var queue = queueService.GetChatQueueByName(queueName, chat.ChatId);
        if (queue is null)
        {
            queue = new Queue()
            {
                Name = queueName,
                ChatId = chat.Id,
                CreatorId = user.Id,
            };

            await queueService.AddAsync(queue);
            var callbackButtonData = new CallbackData()
            {
                Command = CallbackConstants.EnqueueMeCommand,
                ChatId = chat.Id,
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
                chat.ChatId,
                messageProvider.GetMessage(MessageKeys.CreateQueueMessageHandler.CreateQueueCommand_PublicChat_QueueNameTooLong_Message, queue.Name),
                ParseMode.Html,
                replyMarkup: replyMarkup);
        }

        return await botClient.SendTextMessageAsync(
                chat.ChatId,
                messageProvider.GetMessage(MessageKeys.CreateQueueMessageHandler.CreateQueueCommand_PublicChat_QueueAlreadyExists_Message, queue.Name),
                ParseMode.Html);
    }

    private static bool QueueHasNumberAtTheEnd(string[] messageWords)
    {
        return int.TryParse(messageWords[^1], out int _);
    }

    private static bool ChatHasMaximalNumberOfQueues(IChatService chatService, Chat chat, IBotConfiguration botConfiguration)
    {
        return chatService.GetNumberOfQueues(chat.ChatId) >= botConfiguration.QueuesPerChat;
    }
}
