using System.Threading.Tasks;
using Enqueuer.Callbacks.Exceptions;
using Enqueuer.Callbacks.Factories;
using Enqueuer.Data;
using Enqueuer.Data.DataSerialization;
using Enqueuer.Data.TextProviders;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Enqueuer.Callbacks;

public class CallbackDistributor : ICallbackDistributor
{
    private readonly ICallbackHandlersFactory _callbackHandlersFactory;
    private readonly IDataDeserializer _dataDeserializer;
    private readonly ITelegramBotClient _telegramBotClient;
    private readonly IMessageProvider _messageProvider;

    public CallbackDistributor(ICallbackHandlersFactory callbackHandlersFactory, IDataDeserializer dataDeserializer, ITelegramBotClient telegramBotClient, IMessageProvider messageProvider)
    {
        _callbackHandlersFactory = callbackHandlersFactory;
        _dataDeserializer = dataDeserializer;
        _telegramBotClient = telegramBotClient;
        _messageProvider = messageProvider;
    }

    public async Task DistributeAsync(CallbackQuery callbackQuery)
    {
        if (callbackQuery.Message == null)
        {
            return;
        }

        CallbackData? callbackData = null;
        if (callbackQuery.Data != null)
        {
            try
            {
                callbackData = _dataDeserializer.Deserialize<CallbackData>(callbackQuery.Data);
            }
            catch (OutdatedCallbackException)
            {
                await _telegramBotClient.EditMessageTextAsync(
                callbackQuery.Message.Chat,
                callbackQuery.Message.MessageId,
                _messageProvider.GetMessage(CallbackMessageKeys.OutdatedCallback_Message),
                ParseMode.Html);
                return;
            }
        }

        var callback = new Callback(callbackQuery, callbackData);
        if (_callbackHandlersFactory.TryCreateCallbackHandler(callback, out var callbackHandler))
        {
            await callbackHandler.HandleAsync(callback);
        }
    }
}
