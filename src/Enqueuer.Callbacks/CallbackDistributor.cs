using System.Threading;
using System.Threading.Tasks;
using Enqueuer.Callbacks.Factories;
using Enqueuer.Core.Serialization;
using Enqueuer.Data;
using Enqueuer.Data.Exceptions;
using Enqueuer.Data.TextProviders;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Enqueuer.Callbacks;

public class CallbackDistributor : ICallbackDistributor
{
    private readonly ICallbackHandlersFactory _callbackHandlersFactory;
    private readonly ICallbackDataDeserializer _callbackDataDeserializer;
    private readonly ITelegramBotClient _telegramBotClient;
    private readonly IMessageProvider _messageProvider;

    public CallbackDistributor(ICallbackHandlersFactory callbackHandlersFactory, ICallbackDataDeserializer callbackDataDeserializer, ITelegramBotClient telegramBotClient, IMessageProvider messageProvider)
    {
        _callbackHandlersFactory = callbackHandlersFactory;
        _callbackDataDeserializer = callbackDataDeserializer;
        _telegramBotClient = telegramBotClient;
        _messageProvider = messageProvider;
    }

    public async Task DistributeAsync(CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        if (callbackQuery.Message == null)
        {
            return;
        }

        CallbackData? callbackData;
        try
        {
            callbackData = _callbackDataDeserializer.Deserialize(callbackQuery.Data);
        }
        catch (OutdatedCallbackException)
        {
            await _telegramBotClient.EditMessageTextAsync(
                callbackQuery.Message.Chat,
                callbackQuery.Message.MessageId,
                _messageProvider.GetMessage(CallbackMessageKeys.OutdatedCallback_Message),
                ParseMode.Html,
                cancellationToken: cancellationToken);

            return;
        }

        var callback = new Callback(callbackQuery, callbackData);
        if (_callbackHandlersFactory.TryCreateCallbackHandler(callback, out var callbackHandler))
        {
            await callbackHandler.HandleAsync(callback, cancellationToken);
        }
    }
}
