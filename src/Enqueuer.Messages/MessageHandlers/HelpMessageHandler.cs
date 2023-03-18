using System.Threading.Tasks;
using Enqueuer.Data.TextProviders;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Enqueuer.Messages.MessageHandlers;

public class HelpMessageHandler : IMessageHandler
{
    private readonly ITelegramBotClient _botClient;
    private readonly IMessageProvider _messageProvider;

    public HelpMessageHandler(ITelegramBotClient botClient, IMessageProvider messageProvider)
    {
        _botClient = botClient;
        _messageProvider = messageProvider;
    }

    public Task HandleAsync(Message message)
    {
        return _botClient.SendTextMessageAsync(
            message.Chat,
            _messageProvider.GetMessage(MessageKeys.HelpMessageHandler.HelpCommand_Message),
            ParseMode.Html);
    }
}
