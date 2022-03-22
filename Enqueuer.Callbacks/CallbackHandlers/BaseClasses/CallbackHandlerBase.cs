using Enqueuer.Data;
using Enqueuer.Data.DataSerialization;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Enqueuer.Callbacks.CallbackHandlers.BaseClasses
{
    /// <summary>
    /// Contains basic implementation for callback handlers.
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
            this.DataSerializer = dataSerializer;
        }

        /// <inheritdoc/>
        public abstract string Command { get; }

        /// <inheritdoc/>
        public abstract Task<Message> HandleCallbackAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, CallbackData callbackData);
    }
}
