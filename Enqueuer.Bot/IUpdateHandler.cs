using Telegram.Bot.Types;

namespace Enqueuer.Bot
{
    /// <summary>
    /// Handles incoming <see cref="Update"/>.
    /// </summary>
    public interface IUpdateHandler
    {
        /// <summary>
        /// Handles incoming <see cref="Update"/>.
        /// </summary>
        /// <param name="update"><see cref="Update"/> to handle.</param>
        /// <returns>Update handling task.</returns>
        public Task HadnleUpdateAsync(Update update);
    }
}
