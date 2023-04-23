using Enqueuer.Telegram.Core;
using Enqueuer.Telegram.Core.Constants;
using Enqueuer.Telegram.Core.Localization;
using Enqueuer.Telegram.Core.Serialization;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace Enqueuer.Telegram.Callbacks.CallbackHandlers.BaseClasses;

/// <summary>
/// Contains basic implementation for callback handlers with return to chat button.
/// </summary>
public abstract class CallbackHandlerBaseWithReturnToChatButton : CallbackHandlerBase
{
    protected CallbackHandlerBaseWithReturnToChatButton(ITelegramBotClient telegramBotClient, ICallbackDataSerializer dataSerializer, ILocalizationProvider localizationProvider)
        : base(telegramBotClient, dataSerializer, localizationProvider)
    {
    }

    /// <summary>
    /// Gets the return to chat button.
    /// </summary>
    protected InlineKeyboardButton GetReturnToChatButton(CallbackData callbackData)
    {
        var buttonCallbackData = new CallbackData()
        {
            Command = CallbackCommands.GetChatCommand,
            TargetChatId = callbackData.TargetChatId,
        };

        var serializedCallbackData = DataSerializer.Serialize(buttonCallbackData);
        return InlineKeyboardButton.WithCallbackData(LocalizationProvider.GetMessage(CallbackMessageKeys.Callback_Return_Button, MessageParameters.None), serializedCallbackData);
    }
}
