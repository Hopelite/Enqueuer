using System.Threading;
using System.Threading.Tasks;
using Enqueuer.Telegram.Callbacks.Factories;
using Enqueuer.Telegram.Core;
using Enqueuer.Telegram.Core.Exceptions;
using Enqueuer.Telegram.Core.Localization;
using Enqueuer.Telegram.Core.Serialization;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Enqueuer.Telegram.Callbacks;

public class CallbackDistributor : ICallbackDistributor
{
    private readonly ICallbackHandlersFactory _callbackHandlersFactory;
    private readonly ICallbackDataDeserializer _callbackDataDeserializer;
    private readonly ITelegramBotClient _telegramBotClient;
    private readonly ILocalizationProvider _localizationProvider;

    public CallbackDistributor(ICallbackHandlersFactory callbackHandlersFactory, ICallbackDataDeserializer callbackDataDeserializer, ITelegramBotClient telegramBotClient, ILocalizationProvider localizationProvider)
    {
        _callbackHandlersFactory = callbackHandlersFactory;
        _callbackDataDeserializer = callbackDataDeserializer;
        _telegramBotClient = telegramBotClient;
        _localizationProvider = localizationProvider;
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
                _localizationProvider.GetMessage(CallbackMessageKeys.Callback_OutdatedCallback_Message, MessageParameters.None),
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
