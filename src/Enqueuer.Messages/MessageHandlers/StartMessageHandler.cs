using System;
using System.Threading;
using System.Threading.Tasks;
using Enqueuer.Data;
using Enqueuer.Data.Constants;
using Enqueuer.Data.DataSerialization;
using Enqueuer.Data.TextProviders;
using Enqueuer.Messages.Extensions;
using Enqueuer.Services;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Enqueuer.Messages.MessageHandlers;

public class StartMessageHandler : MessageHandlerBase
{
    public StartMessageHandler(IServiceScopeFactory scopeFactory)
        : base(scopeFactory)
    {
    }

    protected override Task HandleAsyncImplementation(IServiceProvider serviceProvider, ITelegramBotClient botClient, Message message)
    {
        var messageProvider = serviceProvider.GetRequiredService<IMessageProvider>();
        if (!message.IsFromPrivateChat())
        {
            return botClient.SendTextMessageAsync(
                message.Chat,
                messageProvider.GetMessage(MessageKeys.StartMessageHanlder.StartCommand_PublicChat_Message),
                ParseMode.Html);
        }

        return HandlePrivateChatAsync(serviceProvider, botClient, messageProvider, message);
    }

    private async Task HandlePrivateChatAsync(IServiceProvider serviceProvider, ITelegramBotClient botClient, IMessageProvider messageProvider, Message message)
    {
        var userService = serviceProvider.GetRequiredService<IUserService>();
        await userService.GetOrStoreUserAsync(message.From!, CancellationToken.None);

        var callbackButtonData = new CallbackData()
        {
            Command = CallbackConstants.ListChatsCommand,
        };

        var dataSerializer = serviceProvider.GetRequiredService<IDataSerializer>();
        var serializedCallbackData = dataSerializer.Serialize(callbackButtonData);

        var viewChatsButton = new InlineKeyboardMarkup(new InlineKeyboardButton[]
        {
            InlineKeyboardButton.WithCallbackData(messageProvider.GetMessage(MessageKeys.StartMessageHanlder.StartCommand_PrivateChat_ListChatsButton), serializedCallbackData),
        });

        await botClient.SendTextMessageAsync(
            message.Chat,
            messageProvider.GetMessage(MessageKeys.StartMessageHanlder.StartCommand_PrivateChat_Message),
            ParseMode.Html,
            replyMarkup: viewChatsButton);
    }
}
