using Enqueuer.Data;
using Enqueuer.Data.Constants;
using Enqueuer.Data.DataSerialization;
using Telegram.Bot.Types.ReplyMarkups;

namespace Enqueuer.Callbacks.CallbackHandlers.BaseClasses
{
    public abstract class CallbackHandlerBaseWithRemoveQueueButton : CallbackHandlerBaseWithReturnToQueueButton
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CallbackHandlerBaseWithRemoveQueueButton"/> class.
        /// </summary>
        /// <param name="dataSerializer"><see cref="IDataSerializer"/> to serialize with.</param>
        public CallbackHandlerBaseWithRemoveQueueButton(IDataSerializer dataSerializer)
            : base(dataSerializer)
        {
        }

        /// <summary>
        /// Gets remove queue button.
        /// </summary>
        /// <param name="buttonText">Button text.</param>
        /// <param name="callbackData"><see cref="CallbackData"/> to rely on.</param>
        /// <param name="isAgreed">Value indicating whether user is agreed to delete queue or not. Null, if user wasn't prompted to delete queue yet.</param>
        /// <returns>Remove queue <see cref="InlineKeyboardButton"/>.</returns>
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

            var serializedCallbackData = this.DataSerializer.Serialize(buttonCallbackData);
            return InlineKeyboardButton.WithCallbackData(buttonText, serializedCallbackData);
        }
    }
}
