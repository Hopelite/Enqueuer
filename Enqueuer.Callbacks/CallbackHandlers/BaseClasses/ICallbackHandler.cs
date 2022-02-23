using Enqueuer.Data;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Enqueuer.Callbacks.CallbackHandlers.BaseClasses
{
    /// <summary>
    /// Handles incoming <see cref="CallbackQuery"/>.
    /// </summary>
    public interface ICallbackHandler
    {
        /// <summary>
        /// Gets command this callback handler responds to.
        /// </summary>
        public string Command { get; }

        /// <summary>
        /// Handles incoming <paramref name="callbackQuery"/>.
        /// </summary>
        /// <param name="botClient"><see cref="ITelegramBotClient"/> to use.</param>
        /// <param name="callbackQuery">Incoming <see cref="CallbackQuery"/> to handle.</param>
        /// <param name="callbackData"><see cref="CallbackData"/> from <paramref name="callbackQuery"/> to work with.</param>
        /// <returns><see cref="Message"/> which was sent in responce.</returns>
        public Task<Message> HandleCallbackAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, CallbackData callbackData);
    }
}
