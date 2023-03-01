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

    protected override Task HandleImplementationAsync(IServiceScope serviceScope, ITelegramBotClient botClient, Message message)
    {
        var messageProvider = serviceScope.ServiceProvider.GetRequiredService<IMessageProvider>();
        return botClient.SendTextMessageAsync(message.Chat, messageProvider.GetMessage(TextKeys.HelpCommand_Message), ParseMode.Html);
    }
}
