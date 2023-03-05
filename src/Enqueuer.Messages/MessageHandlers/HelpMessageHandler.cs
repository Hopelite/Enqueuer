using System;
using System.Threading.Tasks;
using Enqueuer.Data.TextProviders;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Enqueuer.Messages.MessageHandlers;

public class HelpMessageHandler : MessageHandlerBase
{
    public HelpMessageHandler(IServiceScopeFactory scopeFactory)
        : base(scopeFactory)
    {
    }

    protected override Task HandleAsyncImplementation(IServiceProvider serviceProvider, ITelegramBotClient botClient, Message message)
    {
        var messageProvider = serviceProvider.GetRequiredService<IMessageProvider>();
        return botClient.SendTextMessageAsync(message.Chat, messageProvider.GetMessage(MessageKeys.HelpMessageHandler.HelpCommand_Message), ParseMode.Html);
    }
}
