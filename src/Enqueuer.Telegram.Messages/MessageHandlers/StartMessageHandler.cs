using System.Threading;
using System.Threading.Tasks;
using Enqueuer.Messaging.Core;
using Enqueuer.Messaging.Core.Constants;
using Enqueuer.Messaging.Core.Localization;
using Enqueuer.Messaging.Core.Serialization;
using Enqueuer.Messaging.Core.Types.Messages;
using Enqueuer.Services;
using Enqueuer.Telegram.Messages.Extensions;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Enqueuer.Telegram.Messages.MessageHandlers;

public class StartMessageHandler : IMessageHandler
{
    private readonly ITelegramBotClient _botClient;
    private readonly ILocalizationProvider _localizationProvider;
    private readonly IUserService _userService;
    private readonly ICallbackDataSerializer _dataSerializer;

    public StartMessageHandler(ITelegramBotClient botClient, ILocalizationProvider localizationProvider, IUserService userService, ICallbackDataSerializer dataSerializer)
    {
        _botClient = botClient;
        _localizationProvider = localizationProvider;
        _userService = userService;
        _dataSerializer = dataSerializer;
    }

    public Task HandleAsync(MessageContext messageContext, CancellationToken cancellationToken)
    {
        if (!messageContext.IsFromPrivateChat())
        {
            return _botClient.SendTextMessageAsync(
                messageContext.Chat.Id,
                _localizationProvider.GetMessage(MessageKeys.StartMessageHandler.Message_StartCommand_PublicChat_Message, MessageParameters.None),
                ParseMode.Html,
                cancellationToken: cancellationToken);
        }

        return HandlePrivateChatAsync(messageContext, cancellationToken);
    }

    private async Task HandlePrivateChatAsync(MessageContext messageContext, CancellationToken cancellationToken)
    {
        await _userService.GetOrStoreUserAsync(messageContext.Sender, cancellationToken);

        var callbackButtonData = new CallbackData()
        {
            Command = CallbackCommands.ListChatsCommand,
        };

        var serializedCallbackData = _dataSerializer.Serialize(callbackButtonData);

        var viewChatsButton = new InlineKeyboardMarkup(new InlineKeyboardButton[]
        {
            InlineKeyboardButton.WithCallbackData(_localizationProvider.GetMessage(MessageKeys.StartMessageHandler.Message_StartCommand_PrivateChat_ListChats_Button, MessageParameters.None), serializedCallbackData),
        });

        await _botClient.SendTextMessageAsync(
            messageContext.Chat.Id,
            _localizationProvider.GetMessage(MessageKeys.StartMessageHandler.Message_StartCommand_PrivateChat_Message, MessageParameters.None),
            ParseMode.Html,
            replyMarkup: viewChatsButton,
            cancellationToken: cancellationToken);
    }
}
