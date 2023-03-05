using System.Threading;
using System.Threading.Tasks;
using Enqueuer.Persistence.Models;

namespace Enqueuer.Services;

public interface IGroupService
{
    /// <summary>
    /// Gets an already existing <see cref="Group"/> or stores a new <paramref name="telegramGroup"/>.
    /// </summary>
    Task<Group> GetOrStoreGroupAsync(Telegram.Bot.Types.Chat telegramGroup, CancellationToken cancellationToken);

    /// <summary>
    /// Adds or updates <paramref name="user"/> to a new or existing <paramref name="telegramGroup"/>.
    /// </summary>
    Task<(Group group, User user)> AddOrUpdateUserAndGroupAsync(Telegram.Bot.Types.Chat telegramGroup, Telegram.Bot.Types.User user, bool includeQueues, CancellationToken cancellationToken);
}
