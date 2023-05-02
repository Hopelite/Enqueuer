using System.Threading;
using System.Threading.Tasks;
using Enqueuer.Service.API.Services.Types;
using Enqueuer.Service.Messages.Models;

namespace Enqueuer.Service.API.Services;

public interface IGroupService
{
    /// <summary>
    /// Gets the <see cref="GroupInfo"/> of a group with the specified <paramref name="groupId"/> or null, if doesn't exist.
    /// </summary>
    Task<GroupInfo?> GetGroupAsync(long groupId, CancellationToken cancellationToken);

    /// <summary>
    /// Adds a new or updates an existing <paramref name="group"/>.
    /// </summary>
    /// <returns><see cref="PutActionResponse"/>, which indicates, whether the <paramref name="group"/> was added or updated.</returns>
    Task<PutActionResponse> AddOrUpdateGroupAsync(long groupId, Group group, CancellationToken cancellationToken);

    /// <summary>
    /// Gets a user with the specified <paramref name="userId"/>, who participates in a group with the <paramref name="groupId"/> ID.
    /// </summary>
    Task<User?> GetGroupMemberAsync(long groupId, long userId, CancellationToken cancellationToken);

    /// <summary>
    /// Adds a <paramref name="user"/> to a group with the specified <paramref name="id"/>.
    /// </summary>
    /// <returns><see cref="PutActionResponse"/>, which indicates, whether the <paramref name="user"/> was added or updated.</returns>
    Task<PutActionResponse> AddOrUpdateGroupMemberAsync(long id, long userId, User user, CancellationToken cancellationToken);
}
