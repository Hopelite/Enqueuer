using Enqueuer.Core;
using Enqueuer.Core.Constants;
using Enqueuer.Core.Serialization;
using Enqueuer.Core.TextProviders;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace Enqueuer.Callbacks.CallbackHandlers.BaseClasses;

/// <summary>
/// Contains basic implementation for callback handlers with return to chat button.
/// </summary>
public abstract class CallbackHandlerBaseWithReturnToChatButton : CallbackHandlerBase
{
    protected CallbackHandlerBaseWithReturnToChatButton(ITelegramBotClient telegramBotClient, ICallbackDataSerializer dataSerializer, IMessageProvider messageProvider)
        : base(telegramBotClient, dataSerializer, messageProvider)
    {
    }

    /// <summary>
    /// Gets the return to chat button.
    /// </summary>
    protected InlineKeyboardButton GetReturnToChatButton(CallbackData callbackData)
    {
        var buttonCallbackData = new CallbackData()
        {
            Command = CallbackConstants.GetChatCommand,
            TargetChatId = callbackData.TargetChatId,
        };

        var serializedCallbackData = DataSerializer.Serialize(buttonCallbackData);
        return InlineKeyboardButton.WithCallbackData(MessageProvider.GetMessage(CallbackMessageKeys.Return_Button), serializedCallbackData);
    }
}
