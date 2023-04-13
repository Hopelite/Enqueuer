using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Enqueuer.Persistence.Models;
using Enqueuer.Services.Responses;

namespace Enqueuer.Services;

public interface IGroupService
{
    /// <summary>
    /// Gets an already existing <see cref="Group"/> or stores a new <paramref name="telegramGroup"/>.
    /// </summary>
    Task<Group> GetOrStoreGroupAsync(Telegram.Bot.Types.Chat telegramGroup, CancellationToken cancellationToken);

    /// <summary>
    /// Adds to or updates <paramref name="telegramUser"/> in a new or existing <paramref name="telegramGroup"/>.
    /// </summary>
    Task<GetUserGroupResponse> AddOrUpdateUserAndGroupAsync(Telegram.Bot.Types.Chat telegramGroup, Telegram.Bot.Types.User telegramUser, bool includeQueues, CancellationToken cancellationToken);

    /// <summary>
    /// Gets all <see cref="Group"/>s in which the user with the specified <paramref name="userId"/> participates.
    /// </summary>
    Task<IEnumerable<Group>> GetUserGroups(long userId, CancellationToken cancellationToken);

    /// <summary>
    /// 
    /// </summary>
    Task<bool> DoesGroupExist(long groupId);
}
