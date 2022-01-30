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

        /// <summary>
        /// Adds <paramref name="user"/> to <paramref name="chat"/> if is not already added.
        /// </summary>
        /// <param name="user"><see cref="User"/> to add into <paramref name="chat"/>.</param>
        /// <param name="chat"><see cref="Chat"/> where <paramref name="user"/> should be added.</param>
        /// <returns>Task in return.</returns>
        public Task AddUserToChat(User user, Chat chat);
    }
}
