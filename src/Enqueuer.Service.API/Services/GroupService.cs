using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Enqueuer.Persistence;
using Enqueuer.Service.API.Services.Exceptions;
using Enqueuer.Service.API.Services.Types;
using Enqueuer.Service.Messages.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Enqueuer.Service.API.Services;

public class GroupService : IGroupService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IMapper _mapper;

    public GroupService(IServiceScopeFactory scopeFactory, IMapper mapper)
    {
        _scopeFactory = scopeFactory;
        _mapper = mapper;
    }

    public async Task<GroupInfo?> GetGroupAsync(long id, CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var enqueuerContext = scope.ServiceProvider.GetRequiredService<EnqueuerContext>();

        var group = await enqueuerContext.Groups
            .Include(g => g.Queues)
            .FirstOrDefaultAsync(g => g.Id == id, cancellationToken);

        return group == null 
            ? null 
            : _mapper.Map<GroupInfo>(group);
    }

    public async Task<Group[]> GetUserGroupsAsync(long userId, CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var enqueuerContext = scope.ServiceProvider.GetRequiredService<EnqueuerContext>();

        var user = await enqueuerContext.Users
            .Include(u => u.Groups)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null)
        {
            throw new UserDoesNotExistException($"User with the \"{userId}\" ID does not exist.");
        }

        return user.Groups == null 
            ? Array.Empty<Group>()
            : _mapper.Map<Group[]>(user.Groups);
    }

    public Task<GroupInfo> AddOrUpdateAsync(long groupId, Group group, CancellationToken cancellationToken)
    {
        if (group == null)
        {
            throw new ArgumentNullException(nameof(group));
        }

        return AddOrUpdateAsyncInternal(groupId, group, cancellationToken);
    }

    private async Task<GroupInfo> AddOrUpdateAsyncInternal(long groupId, Group group, CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var enqueuerContext = scope.ServiceProvider.GetRequiredService<EnqueuerContext>();

        var existingGroup = await enqueuerContext.Groups
            .Include(g => g.Queues)
            .FirstOrDefaultAsync(g => g.Id == groupId, cancellationToken);

        if (existingGroup == null)
        {
            enqueuerContext.Groups.Add(_mapper.Map<Persistence.Models.Group>(group));
            await enqueuerContext.SaveChangesAsync(cancellationToken);
            return _mapper.Map<GroupInfo>(group);
        }

        if (existingGroup.Title != group.Title)
        {
            existingGroup.Title = group.Title;
            enqueuerContext.Update(group);
        }

        return _mapper.Map<GroupInfo>(existingGroup);
    }

    public async Task<User?> GetGroupMemberAsync(long groupId, long userId, CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var enqueuerContext = scope.ServiceProvider.GetRequiredService<EnqueuerContext>();

        var group = await enqueuerContext.Groups
            .Include(g => g.Members)
            .FirstOrDefaultAsync(g => g.Id == groupId, cancellationToken);

        if (group == null)
        {
            throw new GroupDoesNotExistException($"Group with the \"{groupId}\" ID does not exist.");
        }

        var user = group.Members.FirstOrDefault(m => m.Id == userId);
        return user == null
            ? null
            : _mapper.Map<User>(user);
    }

    public Task<PutActionStatus> AddOrUpdateGroupMemberAsync(long id, long userId, User user, CancellationToken cancellationToken)
    {
        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        return AddOrUpdateMemberAsyncInternal(id, userId, user, cancellationToken);
    }

    private async Task<PutActionStatus> AddOrUpdateMemberAsyncInternal(long id, long userId, User user, CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var enqueuerContext = scope.ServiceProvider.GetRequiredService<EnqueuerContext>();

        var group = await enqueuerContext.Groups
            .Include(g => g.Members)
            .FirstOrDefaultAsync(g => g.Id == id, cancellationToken);

        if (group == null)
        {
            throw new GroupDoesNotExistException($"Group with the \"{id}\" ID does not exist.");
        }

        var existingUser = group.Members.FirstOrDefault(m => m.Id == userId);
        if (existingUser == null)
        {
            // TODO: consider throwing exception, if user does not exist in db at all
            existingUser = _mapper.Map<Persistence.Models.User>(user);
            group.Members.Add(existingUser);

            await enqueuerContext.SaveChangesAsync(cancellationToken);
            return PutActionStatus.Created;
        }

        if (UpdateUserIfNeeded(existingUser, user))
        {
            await enqueuerContext.SaveChangesAsync(cancellationToken);
            return PutActionStatus.Updated;
        }

        return PutActionStatus.None;

        static bool UpdateUserIfNeeded(Persistence.Models.User existingUser, User newUser)
        {
            if (string.Equals(existingUser.FirstName, newUser.FirstName) && string.Equals(existingUser.LastName, newUser.LastName))
            {
                return false;
            }

            existingUser.FirstName = newUser.FirstName;
            existingUser.LastName = newUser.LastName;

            return true;
        }
    }
}
