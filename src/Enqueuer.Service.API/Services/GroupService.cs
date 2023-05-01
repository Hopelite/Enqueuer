using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Enqueuer.Persistence;
using Enqueuer.Service.API.Services.Exceptions;
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
            .SingleOrDefaultAsync(cancellationToken);

        return group == null 
            ? null 
            : _mapper.Map<GroupInfo>(group);
    }

    public async Task<Group[]> GetUserGroupsAsync(long userId, CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var enqueuerContext = scope.ServiceProvider.GetRequiredService<EnqueuerContext>();

        var user = await enqueuerContext.Users.Include(u => u.Groups)
            .SingleOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null)
        {
            throw new UserDoesNotExistException($"User with the \"{userId}\" ID does not exist.");
        }

        return user.Groups == null 
            ? Array.Empty<Group>()
            : _mapper.Map<Group[]>(user.Groups);
    }

    public Task<GroupInfo> AddOrUpdateAsync(Group group, CancellationToken cancellationToken)
    {
        if (group == null)
        {
            throw new ArgumentNullException(nameof(group));
        }

        return AddOrUpdateAsyncInternal(group, cancellationToken);
    }

    private async Task<GroupInfo> AddOrUpdateAsyncInternal(Group group, CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var enqueuerContext = scope.ServiceProvider.GetRequiredService<EnqueuerContext>();

        var existingGroup = await enqueuerContext.Groups
            .Include(g => g.Queues)
            .SingleOrDefaultAsync(cancellationToken);

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
}
