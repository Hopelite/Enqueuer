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
    Task<(Group group, User user)> AddOrUpdateUserToGroupAsync(Telegram.Bot.Types.Chat telegramGroup, Telegram.Bot.Types.User user, bool includeQueues, CancellationToken cancellationToken);


    /// <summary>
    /// Gets <see cref="Group"/> with the specified <paramref name="id"/>.
    /// </summary>
    Task<Group?> GetAsync(long id, CancellationToken cancellationToken);

    /// <summary>
    /// Adds <paramref name="user"/> to <paramref name="chat"/> if is not already added.
    /// </summary>
    /// <param name="user"><see cref="User"/> to add into <paramref name="chat"/>.</param>
    /// <param name="chat"><see cref="Group"/> where <paramref name="user"/> should be added.</param>
    /// <returns>Task in return.</returns>
    public Task AddUserToChatIfNotAlready(User user, Group chat);

    /// <summary>
    /// Gets number of <see cref="Queue"/> is <see cref="Group"/> with specified <paramref name="chatId"/>.
    /// </summary>
    /// <param name="chatId">ID of the Telegram chat whose queues to count.</param>
    /// <returns>Number of queues in chat.</returns>
    public int GetNumberOfQueues(long chatId);
}
