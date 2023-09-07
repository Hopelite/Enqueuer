using Enqueuer.Messaging.Core;
using Enqueuer.Messaging.Core.Constants;
using Enqueuer.Messaging.Core.Localization;
using Enqueuer.Messaging.Core.Serialization;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace Enqueuer.Telegram.Callbacks.CallbackHandlers.BaseClasses;

/// <summary>
/// Contains basic implementation for callback handlers with return to chat and return to queue buttons.
/// </summary>
public abstract class CallbackHandlerBaseWithReturnToQueueButton : CallbackHandlerBaseWithReturnToChatButton
{
    protected CallbackHandlerBaseWithReturnToQueueButton(ITelegramBotClient telegramBotClient, ICallbackDataSerializer dataSerializer, ILocalizationProvider localizationProvider)
        : base(telegramBotClient, dataSerializer, localizationProvider)
    {
    }

    /// <summary>
    /// Gets the return to queue button.
    /// </summary>
    protected InlineKeyboardButton GetReturnToQueueButton(CallbackData callbackData)
    {
        var buttonCallbackData = new CallbackData()
        {
            Command = CallbackCommands.GetQueueCommand,
            TargetChatId = callbackData.TargetChatId,
            QueueData = callbackData.QueueData,
        };

        var serializedCallbackData = DataSerializer.Serialize(buttonCallbackData);
        return InlineKeyboardButton.WithCallbackData(LocalizationProvider.GetMessage(CallbackMessageKeys.Callback_Return_Button, MessageParameters.None), serializedCallbackData);
    }

    protected InlineKeyboardButton GetQueueRelatedButton(string buttonText, string command, CallbackData callbackData, int queueId)
    {
        var buttonCallbackData = new CallbackData()
        {
            Command = command,
            TargetChatId = callbackData.TargetChatId,
            QueueData = new QueueData()
            {
                QueueId = queueId,
            }
        };

        var serializedCallbackData = DataSerializer.Serialize(buttonCallbackData);
        return InlineKeyboardButton.WithCallbackData(buttonText, serializedCallbackData);
    }
}
