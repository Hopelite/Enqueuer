using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Enqueuer.Persistence.Models;
using User = Enqueuer.Persistence.Models.User;

namespace Enqueuer.Services;

public interface IUserService
{
    /// <summary>
    /// Gets an already existing <see cref="User"/> or stores a new <paramref name="telegramChat"/>.
    /// </summary>
    Task<User> GetOrStoreUserAsync(Telegram.Bot.Types.User telegramUser, CancellationToken cancellationToken);

    /// <summary>
    /// Gets user with specified <paramref name="userId"/>.
    /// </summary>
    /// <param name="userId">Telegram user ID to get user by.</param>
    /// <returns><see cref="User"/> with specified <paramref name="userId"/>; null if doesn't exist.</returns>
    public User GetUserByUserId(long userId);

    /// <summary>
    /// Gets <see cref="IEnumerable{T}"/> of <see cref="Group"/> where both user and bot participate.
    /// </summary>
    /// <param name="userId">ID of the Telegram user whose chats to get.</param>
    /// <returns>Chats user participate in.</returns>
    public IEnumerable<Group> GetUserChats(long userId);
}
