using Enqueuer.Persistence.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using User = Enqueuer.Persistence.Models.User;

namespace Enqueuer.Services.Interfaces
{
    /// <summary>
    /// Contains methods for <see cref="User"/>.
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// Gets already existing <see cref="User"/> or creates new one.
        /// </summary>
        /// <param name="telegramUser">Telegram user using which ID to get or create a new one.</param>
        /// <returns>Task with existing or newly created <see cref="User"/> in return.</returns>
        public Task<User> GetOrCreateUserAsync(Telegram.Bot.Types.User telegramUser);

        /// <summary>
        /// Gets user with specified <paramref name="userId"/>.
        /// </summary>
        /// <param name="userId">Telegram user ID to get user by.</param>
        /// <returns><see cref="User"/> with specified <paramref name="userId"/>; null if doesn't exist.</returns>
        public User GetUserByUserId(long userId);

        /// <summary>
        /// Gets <see cref="IEnumerable{T}"/> of <see cref="Chat"/> where both user and bot participate.
        /// </summary>
        /// <param name="userId">ID of the Telegram user whose chats to get.</param>
        /// <returns>Chats user participate in.</returns>
        public IEnumerable<Chat> GetUserChats(long userId);
    }
}
