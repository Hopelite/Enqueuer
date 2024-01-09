using Enqueuer.Messaging.Core;
using Enqueuer.Messaging.Core.Localization;
using Enqueuer.Messaging.Core.Serialization;
using Telegram.Bot.Types.ReplyMarkups;

namespace Enqueuer.Telegram.Callbacks.Helpers.Markup;

internal class ReturnToChatMarkup
{
    private readonly ICallbackDataSerializer _dataSerializer;
    private readonly ILocalizationProvider _localizationProvider;

    public ReturnToChatMarkup(ICallbackDataSerializer dataSerializer, ILocalizationProvider localizationProvider)
    {
        _dataSerializer = dataSerializer;
        _localizationProvider = localizationProvider;
    }

    public InlineKeyboardMarkup Create(CallbackData callbackData)
        => ReplyMarkupBuilder.Create(_dataSerializer, _localizationProvider)
                .WithReturnToChatButton(callbackData)
                .Build();
}
