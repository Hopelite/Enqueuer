using System.Threading;
using System.Threading.Tasks;
using User = Enqueuer.Persistence.Models.User;

namespace Enqueuer.Services;

public interface IUserService
{
    /// <summary>
    /// Gets an already existing <see cref="User"/> or stores a new <paramref name="telegramUser"/>.
    /// If an existing user's first or last name differs from the <paramref name="telegramUser"/> one's, then updates it.
    /// </summary>
    Task<User> GetOrStoreUserAsync(Telegram.Bot.Types.User telegramUser, CancellationToken cancellationToken);
}
