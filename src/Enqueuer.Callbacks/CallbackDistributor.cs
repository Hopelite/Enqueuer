using System.Threading.Tasks;
using Enqueuer.Callbacks.Factories;
using Enqueuer.Data;
using Enqueuer.Data.DataSerialization;
using Telegram.Bot.Types;

namespace Enqueuer.Callbacks;

public class CallbackDistributor : ICallbackDistributor
{
    private readonly ICallbackHandlersFactory _callbackHandlersFactory;
    private readonly IDataDeserializer _dataDeserializer;

    public CallbackDistributor(ICallbackHandlersFactory callbackHandlersFactory, IDataDeserializer dataDeserializer)
    {
        _callbackHandlersFactory = callbackHandlersFactory;
        _dataDeserializer = dataDeserializer;
    }

    public async Task DistributeAsync(CallbackQuery callbackQuery)
    {
        CallbackData? callbackData = null;
        if (callbackQuery.Data != null)
        {
            callbackData = _dataDeserializer.Deserialize<CallbackData>(callbackQuery.Data);
        }

        var callback = new Callback(callbackQuery, callbackData);
        if (_callbackHandlersFactory.TryCreateCallbackHandler(callback, out var callbackHandler))
        {
            await callbackHandler.HandleAsync(callback);
        }
    }
}
