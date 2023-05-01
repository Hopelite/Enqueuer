using System.Threading;
using System.Threading.Tasks;
using Enqueuer.Service.API.Services.Exceptions;
using Enqueuer.Service.Messages.Models;

namespace Enqueuer.Service.API.Services;

public interface IGroupService
{
    /// <summary>
    /// Gets the <see cref="GroupInfo"/> of a group with the specified <paramref name="id"/> or null, if doesn't exist.
    /// </summary>
    Task<GroupInfo?> GetGroupAsync(long id, CancellationToken cancellationToken);

    /// <summary>
    /// Gets all <see cref="Group"/>s in which the user with the specified <paramref name="userId"/> participates.
    /// </summary>
    /// <exception cref="UserDoesNotExistException" />
    Task<Group[]> GetUserGroupsAsync(long userId, CancellationToken cancellationToken);

    /// <summary>
    /// Adds a new or updates an existing <paramref name="group"/>.
    /// </summary>
    /// <returns><see cref="GroupInfo"/> of the newly added or existing <paramref name="group"/>.</returns>
    Task<GroupInfo> AddOrUpdateAsync(Group group, CancellationToken cancellationToken);
}
