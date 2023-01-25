using System.Threading.Tasks;
using Enqueuer.Data;
using Enqueuer.Data.DataSerialization;
using Enqueuer.Data.Exceptions;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Enqueuer.Callbacks.CallbackHandlers.BaseClasses
{
    /// <summary>
    /// Contains the basic implementation for callback handlers with refresh button.
    /// </summary>
    public abstract class CallbackHandlerBase : ICallbackHandler
    {
        protected readonly IDataSerializer DataSerializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="CallbackHandlerBase"/> class.
        /// </summary>
        /// <param name="dataSerializer"><see cref="IDataSerializer"/> to serialize with.</param>
        public CallbackHandlerBase(IDataSerializer dataSerializer)
        {
            DataSerializer = dataSerializer;
        }

        public abstract string Command { get; }

        public async Task<Message> HandleCallbackAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, CallbackData callbackData)
        {
            try
            {
                return await HandleCallbackAsyncImplementation(botClient, callbackQuery, callbackData);
            }
            catch (MessageNotModifiedException)
            {
                await botClient.AnswerCallbackQueryAsync(callbackQuery.Id, "كل شيء محدث");
                return null;
            }
        }

        protected abstract Task<Message> HandleCallbackAsyncImplementation(ITelegramBotClient botClient, CallbackQuery callbackQuery, CallbackData callbackData);

        protected InlineKeyboardButton GetRefreshButton(CallbackData callbackData)
        {
            var serializedCallbackData = DataSerializer.Serialize(callbackData);
            return InlineKeyboardButton.WithCallbackData("تحديث", serializedCallbackData);
        }
    }
}
