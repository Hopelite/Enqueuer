using System;
using System.Threading.Tasks;
using Enqueuer.Data;
using Enqueuer.Data.Constants;
using Enqueuer.Data.DataSerialization;
using Enqueuer.Data.TextProviders;
using Enqueuer.Messages.Extensions;
using Enqueuer.Services.Interfaces;
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

    protected override Task HandleImplementationAsync(IServiceScope serviceScope, ITelegramBotClient botClient, Message message)
    {
        var messageProvider = serviceScope.ServiceProvider.GetRequiredService<IMessageProvider>();

        if (!message.IsFromPrivateChat())
        {
            return botClient.SendTextMessageAsync(message.Chat, messageProvider.GetMessage(TextKeys.StartCommand_PublicChat_Message), ParseMode.Html);
        }

        return HandlePrivateChatAsync(serviceScope.ServiceProvider, botClient, messageProvider, message);
    }

    private async Task HandlePrivateChatAsync(IServiceProvider serviceProvider, ITelegramBotClient botClient, IMessageProvider messageProvider, Message message)
    {
        var userService = serviceProvider.GetRequiredService<IUserService>();

        await userService.GetOrCreateUserAsync(message.From);
        var callbackButtonData = new CallbackData()
        {
            Command = CallbackConstants.ListChatsCommand,
        };

        var dataSerializer = serviceProvider.GetRequiredService<IDataSerializer>();
        var serializedCallbackData = dataSerializer.Serialize(callbackButtonData);

        var viewChatsButton = new InlineKeyboardMarkup(new InlineKeyboardButton[]
        {
            InlineKeyboardButton.WithCallbackData(messageProvider.GetMessage(TextKeys.StartCommand_PrivateChat_ListChatsButton), serializedCallbackData),
        });

        await botClient.SendTextMessageAsync(
            message.Chat,
            messageProvider.GetMessage(TextKeys.StartCommand_PrivateChat_Message),
            ParseMode.Html,
            replyMarkup: viewChatsButton);
    }
}
