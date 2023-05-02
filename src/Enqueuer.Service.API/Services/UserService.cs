using System;
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

public class UserService : IUserService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IMapper _mapper;

    public UserService(IServiceScopeFactory scopeFactory, IMapper mapper)
    {
        _scopeFactory = scopeFactory;
        _mapper = mapper;
    }

    public async Task<UserInfo?> GetUserAsync(long userId, CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var enqueuerContext = scope.ServiceProvider.GetRequiredService<EnqueuerContext>();

        var user = await enqueuerContext.Users
            .Include(u => u.Groups)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        return user == null
            ? null 
            : _mapper.Map<UserInfo>(user);
    }

    public Task<PutActionResponse> AddOrUpdateUserAsync(long userId, User user, CancellationToken cancellationToken)
    {
        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        return AddOrUpdateUserAsyncInternal(userId, user, cancellationToken);
    }

    private async Task<PutActionResponse> AddOrUpdateUserAsyncInternal(long id, User user, CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var enqueuerContext = scope.ServiceProvider.GetRequiredService<EnqueuerContext>();

        var existingUser = await enqueuerContext.Users
            .Include(u => u.ParticipatesIn)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

        if (existingUser == null)
        {
            existingUser = _mapper.Map<Persistence.Models.User>(user);

            enqueuerContext.Users.Add(existingUser);
            await enqueuerContext.SaveChangesAsync(cancellationToken);

            return PutAction.Created;
        }

        if (UpdateUserIfNeeded(existingUser, user))
        {
            enqueuerContext.Update(existingUser);
            await enqueuerContext.SaveChangesAsync(cancellationToken);
            return PutAction.Updated;
        }

        return PutAction.NoAction;

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
}
