using System.Threading;
using System.Threading.Tasks;
using Enqueuer.Service.API.Services.Exceptions;
using Enqueuer.Service.API.Services.Types;
using Enqueuer.Service.Messages.Models;

namespace Enqueuer.Service.API.Services;

public interface IUserService
{
    /// <summary>
    /// Gets the <see cref="UserInfo"/> of a user with the specified <paramref name="userId"/> or null, if doesn't exist.
    /// </summary>
    Task<UserInfo?> GetUserAsync(long userId, CancellationToken cancellationToken);

    /// <summary>
    /// Adds a new or updates an existing <paramref name="user"/>.
    /// </summary>
    /// <returns><see cref="PutActionResponse"/>, which indicates, whether the <paramref name="user"/> was added or updated.</returns>
    Task<PutActionResponse> AddOrUpdateUserAsync(long userId, User user, CancellationToken cancellationToken);

    /// <summary>
    /// Gets all <see cref="Group"/>s in which the user with the specified <paramref name="userId"/> participates.
    /// </summary>
    /// <exception cref="UserDoesNotExistException" />
    Task<Group[]> GetUserGroupsAsync(long userId, CancellationToken cancellationToken);
}
