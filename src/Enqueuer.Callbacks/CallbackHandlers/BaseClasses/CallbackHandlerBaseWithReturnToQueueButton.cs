using Enqueuer.Data;
using Enqueuer.Data.Constants;
using Enqueuer.Data.DataSerialization;
using Telegram.Bot.Types.ReplyMarkups;

namespace Enqueuer.Callbacks.CallbackHandlers.BaseClasses
{
    /// <summary>
    /// Contains basic implementation for callback handlers with return to chat and return to queue buttons.
    /// </summary>
    public abstract class CallbackHandlerBaseWithReturnToQueueButton : CallbackHandlerBaseWithReturnToChatButton
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CallbackHandlerBaseWithReturnToQueueButton"/> class.
        /// </summary>
        /// <param name="dataSerializer"><see cref="IDataSerializer"/> to serialize with.</param>
        public CallbackHandlerBaseWithReturnToQueueButton(IDataSerializer dataSerializer)
            : base(dataSerializer)
        {
        }

        /// <summary>
        /// Gets return to queue button.
        /// </summary>
        /// <param name="callbackData"><see cref="CallbackData"/> to rely on.</param>
        /// <returns>Return to queue <see cref="InlineKeyboardButton"/>.</returns>
        protected InlineKeyboardButton GetReturnToQueueButton(CallbackData callbackData)
        {
            var buttonCallbackData = new CallbackData()
            {
                Command = CallbackConstants.GetQueueCommand,
                ChatId = callbackData.ChatId,
                QueueData = callbackData.QueueData,
            };

            var serializedCallbackData = this.DataSerializer.Serialize(buttonCallbackData);
            return InlineKeyboardButton.WithCallbackData("Return", serializedCallbackData);
        }
    }
}
