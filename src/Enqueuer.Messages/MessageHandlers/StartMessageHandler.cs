using System.Threading;
using System.Threading.Tasks;
using Enqueuer.Core;
using Enqueuer.Core.Constants;
using Enqueuer.Core.Serialization;
using Enqueuer.Core.TextProviders;
using Enqueuer.Messages.Extensions;
using Enqueuer.Services;
using Enqueuer.Telegram.Core.Localization;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Enqueuer.Messages.MessageHandlers;

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

    public Task HandleAsync(Message message, CancellationToken cancellationToken)
    {
        if (!message.IsFromPrivateChat())
        {
            return _botClient.SendTextMessageAsync(
                message.Chat,
                _localizationProvider.GetMessage(MessageKeys.StartMessageHandler.Message_StartCommand_PublicChat_Message, MessageParameters.None),
                ParseMode.Html,
                cancellationToken: cancellationToken);
        }

        return HandlePrivateChatAsync(message, cancellationToken);
    }

    private async Task HandlePrivateChatAsync(Message message, CancellationToken cancellationToken)
    {
        await _userService.GetOrStoreUserAsync(message.From!, cancellationToken);

        var callbackButtonData = new CallbackData()
        {
            Command = CallbackConstants.ListChatsCommand,
        };

        var serializedCallbackData = _dataSerializer.Serialize(callbackButtonData);

        var viewChatsButton = new InlineKeyboardMarkup(new InlineKeyboardButton[]
        {
            InlineKeyboardButton.WithCallbackData(_localizationProvider.GetMessage(MessageKeys.StartMessageHandler.Message_StartCommand_PrivateChat_ListChats_Button, MessageParameters.None), serializedCallbackData),
        });

        await _botClient.SendTextMessageAsync(
            message.Chat,
            _localizationProvider.GetMessage(MessageKeys.StartMessageHandler.Message_StartCommand_PrivateChat_Message, MessageParameters.None),
            ParseMode.Html,
            replyMarkup: viewChatsButton,
            cancellationToken: cancellationToken);
    }
}
