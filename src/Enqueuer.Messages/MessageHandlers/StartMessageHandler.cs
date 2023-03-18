using System.Threading;
using System.Threading.Tasks;
using Enqueuer.Data;
using Enqueuer.Data.Constants;
using Enqueuer.Data.DataSerialization;
using Enqueuer.Data.TextProviders;
using Enqueuer.Messages.Extensions;
using Enqueuer.Services;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Enqueuer.Messages.MessageHandlers;

public class StartMessageHandler : IMessageHandler
{
    private readonly ITelegramBotClient _botClient;
    private readonly IMessageProvider _messageProvider;
    private readonly IUserService _userService;
    private readonly IDataSerializer _dataSerializer;

    public StartMessageHandler(ITelegramBotClient botClient, IMessageProvider messageProvider, IUserService userService, IDataSerializer dataSerializer)
    {
        _botClient = botClient;
        _messageProvider = messageProvider;
        _userService = userService;
        _dataSerializer = dataSerializer;
    }

    public Task HandleAsync(Message message)
    {
        if (!message.IsFromPrivateChat())
        {
            return _botClient.SendTextMessageAsync(
                message.Chat,
                _messageProvider.GetMessage(MessageKeys.StartMessageHandler.StartCommand_PublicChat_Message),
                ParseMode.Html);
        }

        return HandlePrivateChatAsync(message);
    }

    private async Task HandlePrivateChatAsync(Message message)
    {
        await _userService.GetOrStoreUserAsync(message.From!, CancellationToken.None);

        var callbackButtonData = new CallbackData()
        {
            Command = CallbackConstants.ListChatsCommand,
        };

        var serializedCallbackData = _dataSerializer.Serialize(callbackButtonData);

        var viewChatsButton = new InlineKeyboardMarkup(new InlineKeyboardButton[]
        {
            InlineKeyboardButton.WithCallbackData(_messageProvider.GetMessage(MessageKeys.StartMessageHandler.StartCommand_PrivateChat_ListChatsButton), serializedCallbackData),
        });

        await _botClient.SendTextMessageAsync(
            message.Chat,
            _messageProvider.GetMessage(MessageKeys.StartMessageHandler.StartCommand_PrivateChat_Message),
            ParseMode.Html,
            replyMarkup: viewChatsButton);
    }
}
