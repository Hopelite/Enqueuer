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

    public CallbackDistributor(ICallbackHandlersFactory callbackHandlersFactory, IDataDeserializer dataDeserializer, ITelegramBotClient telegramBotClient)
    {
        _callbackHandlersFactory = callbackHandlersFactory;
        _dataDeserializer = dataDeserializer;
        _telegramBotClient = telegramBotClient;
    }

    public async Task DistributeAsync(CallbackQuery callbackQuery)
    {
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
                "Message is outdated",
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
