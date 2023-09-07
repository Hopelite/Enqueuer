using System.Threading;
using System.Threading.Tasks;
using Enqueuer.Messaging.Core.Localization;
using Enqueuer.Messaging.Core.Types.Messages;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace Enqueuer.Telegram.Messages.MessageHandlers;

public class HelpMessageHandler : IMessageHandler
{
    private readonly ITelegramBotClient _botClient;
    private readonly ILocalizationProvider _localizationProvider;

    public HelpMessageHandler(ITelegramBotClient botClient, ILocalizationProvider localizationProvider)
    {
        _botClient = botClient;
        _localizationProvider = localizationProvider;
    }

    public Task HandleAsync(MessageContext messageContext, CancellationToken cancellationToken)
    {
        return _botClient.SendTextMessageAsync(
            messageContext.Chat.Id,
            _localizationProvider.GetMessage(MessageKeys.HelpMessageHandler.Message_HelpCommand_Message, MessageParameters.None),
            ParseMode.Html,
            cancellationToken: cancellationToken);
    }
}
