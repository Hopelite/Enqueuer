using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Enqueuer.Persistence;
using Enqueuer.Persistence.Models;
using Enqueuer.Services.Extensions;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot.Types.Enums;

namespace Enqueuer.Services;

public class GroupService : IGroupService
{
    private readonly EnqueuerContext _enqueuerContext;

    public GroupService(EnqueuerContext enqueuerContext)
    {
        _enqueuerContext = enqueuerContext;
    }

    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException"/>
    /// <exception cref="ArgumentException">Thrown if <paramref name="telegramGroup"/> is not a Group or Supergroup.</exception>
    public Task<Group> GetOrStoreGroupAsync(Telegram.Bot.Types.Chat telegramGroup, CancellationToken cancellationToken)
    {
        if (telegramGroup == null)
        {
            throw new ArgumentNullException(nameof(telegramGroup));
        }

        if (telegramGroup.Type != ChatType.Group || telegramGroup.Type != ChatType.Supergroup)
        {
            throw new ArgumentException($"{nameof(GroupService)} supports only {ChatType.Group} and {ChatType.Supergroup} chat types.", nameof(telegramGroup));
        }

        return GetOrStoreGroupAsyncInternal(telegramGroup, cancellationToken);
    }

    private async Task<Group> GetOrStoreGroupAsyncInternal(Telegram.Bot.Types.Chat telegramGroup, CancellationToken cancellationToken)
    {
        var group = await _enqueuerContext.FindAsync<Group>(new object[] { telegramGroup.Id }, cancellationToken);
        if (group == null)
        {
            group = telegramGroup.ConvertToGroup();
            _enqueuerContext.Add(group);
            await _enqueuerContext.SaveChangesAsync(cancellationToken);
        }

        return group;
    }

    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException"/>
    public Task<(Group group, User user)> AddOrUpdateUserAndGroupAsync(Telegram.Bot.Types.Chat telegramGroup, Telegram.Bot.Types.User telegramUser, bool includeQueues, CancellationToken cancellationToken)
    {
        if (telegramGroup == null)
        {
            throw new ArgumentNullException(nameof(telegramGroup));
        }

        if (telegramUser == null)
        {
            throw new ArgumentNullException(nameof(telegramUser));
        }

        return AddOrUpdateUserToGroupAsyncInternal(telegramGroup, telegramUser, includeQueues, cancellationToken);
    }

    [SuppressMessage("Blocker Code Smell", "S2178:Short-circuit logic should be used in boolean contexts", Justification = "Both sides are expected to be evaluated.")]
    private async Task<(Group group, User user)> AddOrUpdateUserToGroupAsyncInternal(Telegram.Bot.Types.Chat telegramGroup, Telegram.Bot.Types.User telegramUser, bool includeQueues, CancellationToken cancellationToken)
    {
        var group = await IncludeProperties(_enqueuerContext, includeQueues).SingleOrDefaultAsync(g => g.Id == telegramGroup.Id, cancellationToken);
        var user = await _enqueuerContext.FindAsync<User>(new object[] { telegramUser.Id }, cancellationToken);

        var isSaveNeeded = AddOrUpdateGroup(_enqueuerContext, group, telegramGroup) | AddOrUpdateUser(_enqueuerContext, user, telegramUser);
        if (!group.Members.Any(m => m.Id == user.Id))
        {
            group.Members.Add(user);
            _enqueuerContext.Update(group);
            isSaveNeeded = true;
        }

        if (isSaveNeeded)
        {
            await _enqueuerContext.SaveChangesAsync(cancellationToken);
        }

        return (group, user);

        static IQueryable<Group> IncludeProperties(EnqueuerContext enqueuerContext, bool includeQueues)
        {
            if (includeQueues)
            {
                return enqueuerContext.Groups.Include(g => g.Members)
                        .Include(g => g.Queues);
            }

            return enqueuerContext.Groups.Include(g => g.Members);
        }

        static bool AddOrUpdateGroup(EnqueuerContext enqueuerContext, [NotNull] Group? group, Telegram.Bot.Types.Chat telegramGroup)
        {
            if (group == null)
            {
                group = telegramGroup.ConvertToGroup();
                enqueuerContext.Add(group);
                return true;
            }

            if (group.Title != telegramGroup.Title)
            {
                group.Title = telegramGroup.Title;
                enqueuerContext.Update(group);
                return true;
            }

            return false;
        }

        static bool AddOrUpdateUser(EnqueuerContext enqueuerContext, [NotNull] User? user, Telegram.Bot.Types.User telegramUser)
        {
            if (user == null)
            {
                user = telegramUser.ConvertToEntity();
                enqueuerContext.Add(user);
                return true;
            }

            if (user.FirstName != telegramUser.FirstName || user.LastName != telegramUser.LastName)
            {
                user.FirstName = telegramUser.FirstName;
                user.LastName = telegramUser.LastName;
                enqueuerContext.Update(user);
                return true;
            }

            return false;
        }
    }

    public async Task<IEnumerable<Group>> GetUserGroups(long userId, CancellationToken cancellationToken)
    {
        // TODO: optimize with the direct JOIN table call
        var user = await _enqueuerContext.Users.Include(u => u.Groups)
            .SingleOrDefaultAsync(u => u.Id == userId, cancellationToken);

        return user?.Groups ?? Enumerable.Empty<Group>();
    }
}
