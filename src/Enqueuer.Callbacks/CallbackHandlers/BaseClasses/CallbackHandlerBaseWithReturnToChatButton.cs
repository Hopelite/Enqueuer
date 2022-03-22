using Enqueuer.Data;
using Enqueuer.Data.Constants;
using Enqueuer.Data.DataSerialization;
using Telegram.Bot.Types.ReplyMarkups;

namespace Enqueuer.Callbacks.CallbackHandlers.BaseClasses
{
    /// <summary>
    /// Contains basic implementation for callback handlers with return to chat button.
    /// </summary>
    public abstract class CallbackHandlerBaseWithReturnToChatButton : CallbackHandlerBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CallbackHandlerBaseWithReturnToQueueButton"/> class.
        /// </summary>
        /// <param name="dataSerializer"><see cref="IDataSerializer"/> to serialize with.</param>
        public CallbackHandlerBaseWithReturnToChatButton(IDataSerializer dataSerializer)
            : base(dataSerializer)
        {
        }

        /// <summary>
        /// Gets return to chat button.
        /// </summary>
        /// <param name="callbackData"><see cref="CallbackData"/> to rely on.</param>
        /// <returns>Return to chat <see cref="InlineKeyboardButton"/>.</returns>
        protected InlineKeyboardButton GetReturnToChatButton(CallbackData callbackData)
        {
            var buttonCallbackData = new CallbackData()
            {
                Command = CallbackConstants.GetChatCommand,
                ChatId = callbackData.ChatId,
            };

            var serializedCallbackData = this.DataSerializer.Serialize(buttonCallbackData);
            return InlineKeyboardButton.WithCallbackData("Return", serializedCallbackData);
        }
    }
}
