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
        /// <param name="chatId">Telegram chat ID to get by or create new with.</param>
        /// <returns>Task with existing or newly created <see cref="Chat"/> in return.</returns>
        public Task<Chat> GetNewOrExistingChat(long chatId);
    }
}
