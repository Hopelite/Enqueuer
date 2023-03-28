using Enqueuer.Data;
using Enqueuer.Data.Constants;
using Enqueuer.Data.DataSerialization;
using Enqueuer.Data.TextProviders;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace Enqueuer.Callbacks.CallbackHandlers.BaseClasses;

public abstract class CallbackHandlerBaseWithRemoveQueueButton : CallbackHandlerBaseWithReturnToQueueButton
{
    protected CallbackHandlerBaseWithRemoveQueueButton(ITelegramBotClient telegramBotClient, IDataSerializer dataSerializer, IMessageProvider messageProvider)
        : base(telegramBotClient, dataSerializer, messageProvider)
    {
    }

    /// <summary>
    /// Gets the remove queue button.
    /// </summary>
    /// <param name="isAgreed">Value indicating whether user is agreed to delete queue or not. Null, if user wasn't prompted to delete queue yet.</param>
    protected InlineKeyboardButton GetRemoveQueueButton(string buttonText, CallbackData callbackData, bool? isAgreed = null)
    {
        var buttonCallbackData = new CallbackData()
        {
            Command = CallbackConstants.RemoveQueueCommand,
            ChatId = callbackData.ChatId,
            QueueData = new QueueData()
            {
                QueueId = callbackData.QueueData.QueueId,
                IsUserAgreed = isAgreed
            }
        };

        var serializedCallbackData = DataSerializer.Serialize(buttonCallbackData);
        return InlineKeyboardButton.WithCallbackData(buttonText, serializedCallbackData);
    }
}
