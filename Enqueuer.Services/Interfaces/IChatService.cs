using System.Threading.Tasks;
using Enqueuer.Persistence.Models;

namespace Enqueuer.Services.Interfaces
{
    /// <summary>
    /// Contains methods for <see cref="Chat"/>.
    /// </summary>
    public interface IChatService
    {
        /// <summary>
        /// Gets already existing <see cref="Chat"/> or creates new one.
        /// </summary>
        /// <param name="telegramChat">Telegram chat using which ID to get or create a new one.</param>
        /// <returns>Task with existing or newly created <see cref="Chat"/> in return.</returns>
        public Task<Chat> GetNewOrExistingChatAsync(Telegram.Bot.Types.Chat telegramChat);
    }
}
