using System.Threading;
using System.Threading.Tasks;
using Enqueuer.Core.TextProviders;
using Enqueuer.Telegram.Core.Localization;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Enqueuer.Messages.MessageHandlers;

public class HelpMessageHandler : IMessageHandler
{
    private readonly ITelegramBotClient _botClient;
    private readonly ILocalizationProvider _localizationProvider;

    public HelpMessageHandler(ITelegramBotClient botClient, ILocalizationProvider localizationProvider)
    {
        _botClient = botClient;
        _localizationProvider = localizationProvider;
    }

    public Task HandleAsync(Message message, CancellationToken cancellationToken)
    {
        return _botClient.SendTextMessageAsync(
            message.Chat,
            _localizationProvider.GetMessage(MessageKeys.HelpMessageHandler.Message_HelpCommand_Message, MessageParameters.None),
            ParseMode.Html,
            cancellationToken: cancellationToken);
    }
}
