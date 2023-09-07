using Enqueuer.Messaging.Core;
using Enqueuer.Messaging.Core.Constants;
using Enqueuer.Messaging.Core.Localization;
using Enqueuer.Messaging.Core.Serialization;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace Enqueuer.Telegram.Callbacks.CallbackHandlers.BaseClasses;

public abstract class CallbackHandlerBaseWithRemoveQueueButton : CallbackHandlerBaseWithReturnToQueueButton
{
    protected CallbackHandlerBaseWithRemoveQueueButton(ITelegramBotClient telegramBotClient, ICallbackDataSerializer dataSerializer, ILocalizationProvider localizationProvider)
        : base(telegramBotClient, dataSerializer, localizationProvider)
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
            Command = CallbackCommands.RemoveQueueCommand,
            TargetChatId = callbackData.TargetChatId,
            UserAgreement = isAgreed,
            QueueData = new QueueData()
            {
                QueueId = callbackData.QueueData.QueueId,
            }
        };

        var serializedCallbackData = DataSerializer.Serialize(buttonCallbackData);
        return InlineKeyboardButton.WithCallbackData(buttonText, serializedCallbackData);
    }
}
