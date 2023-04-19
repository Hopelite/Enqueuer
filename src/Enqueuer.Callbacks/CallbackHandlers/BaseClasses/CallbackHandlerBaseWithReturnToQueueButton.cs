using Enqueuer.Core;
using Enqueuer.Core.Constants;
using Enqueuer.Core.Serialization;
using Enqueuer.Core.TextProviders;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace Enqueuer.Callbacks.CallbackHandlers.BaseClasses;

/// <summary>
/// Contains basic implementation for callback handlers with return to chat and return to queue buttons.
/// </summary>
public abstract class CallbackHandlerBaseWithReturnToQueueButton : CallbackHandlerBaseWithReturnToChatButton
{
    protected CallbackHandlerBaseWithReturnToQueueButton(ITelegramBotClient telegramBotClient, ICallbackDataSerializer dataSerializer, IMessageProvider messageProvider)
        : base(telegramBotClient, dataSerializer, messageProvider)
    {
    }

    /// <summary>
    /// Gets the return to queue button.
    /// </summary>
    protected InlineKeyboardButton GetReturnToQueueButton(CallbackData callbackData)
    {
        var buttonCallbackData = new CallbackData()
        {
            Command = CallbackConstants.GetQueueCommand,
            TargetChatId = callbackData.TargetChatId,
            QueueData = callbackData.QueueData,
        };

        var serializedCallbackData = DataSerializer.Serialize(buttonCallbackData);
        return InlineKeyboardButton.WithCallbackData(MessageProvider.GetMessage(CallbackMessageKeys.Return_Button), serializedCallbackData);
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
