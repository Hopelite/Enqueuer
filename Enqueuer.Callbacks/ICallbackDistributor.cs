using System.Threading.Tasks;
using Enqueuer.Callbacks.CallbackHandlers;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Enqueuer.Callbacks
{
    /// <summary>
    /// Distributes callbacks to callback handlers.
    /// </summary>
    public interface ICallbackDistributor
    {
        /// <summary>
        /// Distributes incoming <paramref name="callbackQuery"/> to specified <see cref="ICallbackHandler"/>.
        /// </summary>
        /// <param name="telegramBotClient"><see cref="ITelegramBotClient"/> to use.<</param>
        /// <param name="callbackQuery"><see cref="CallbackQuery"/> to distribute.</param>
        /// <returns>Awaitable <see cref="Task"/>.</returns>
        public Task DistributeCallbackAsync(ITelegramBotClient telegramBotClient, CallbackQuery callbackQuery);
    }
}
