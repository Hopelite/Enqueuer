using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Enqueuer.Persistence;
using Enqueuer.Persistence.Constants;
using Enqueuer.Persistence.Models;
using Enqueuer.Services.Exceptions;
using Enqueuer.Services.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Enqueuer.Services;

public class QueueService : IQueueService
{
    private readonly EnqueuerContext _enqueuerContext;
    private readonly IServiceScopeFactory _scopeFactory;

    public QueueService(EnqueuerContext enqueuerContext, IServiceScopeFactory scopeFactory)
    {
        _enqueuerContext = enqueuerContext;
        _scopeFactory = scopeFactory;
    }

    /// <inheritdoc/>
    /// <exception cref="UserDoesNotExistException" />
    /// <exception cref="ArgumentNullException" />
    /// <exception cref="QueueNameIsTooLongException" />
    /// <exception cref="InvalidQueueNameException" />
    /// <exception cref="InvalidMemberPositionException" />
    public async Task<CreateQueueResponse> CreateQueueAsync(long creatorId, long groupId, string queueName, int? position = null, CancellationToken cancellationToken = default)
    {
        using var scope = _scopeFactory.CreateScope();
        var enqueuerContext = scope.ServiceProvider.GetRequiredService<EnqueuerContext>();

        var creator = await enqueuerContext.Users.FindAsync(new object[] { creatorId }, cancellationToken);
        if (creator == null)
        {
            throw new UserDoesNotExistException($"User with the \"{creatorId}\" ID does not exist.");
        }

        if (string.IsNullOrWhiteSpace(queueName))
        {
            throw new ArgumentNullException(nameof(queueName), "Queue name can't be null, empty or a whitespace.");
        }

        if (queueName.Length > QueueConstants.MaxNameLength)
        {
            throw new QueueNameIsTooLongException(queueName, $"Queue name cannot be longer than {QueueConstants.MaxNameLength}.");
        }

        if (queueName.All(c => char.IsDigit(c)))
        {
            throw new InvalidQueueNameException("Queue name can't contain only digits.");
        }

        if (enqueuerContext.Queues.Any(q => q.GroupId == groupId && q.Name.Equals(queueName)))
        {
            throw new QueueAlreadyExistsException();
        }

        var queue = new Queue
        {
            Name = queueName,
            GroupId = groupId,
            Creator = creator,
            Members = new List<QueueMember>(),
        };

        if (position.HasValue)
        {
            if (position <= 0 || position > QueueConstants.MaxPosition)
            {
                throw new InvalidMemberPositionException($"Member's position can't be negative or greater than {QueueConstants.MaxPosition}.");
            }

            var member = new QueueMember
            {
                Position = position.Value,
                User = creator,
                Queue = queue,
            };

            queue.Members.Add(member);
        }

        await enqueuerContext.Queues.AddAsync(queue, cancellationToken);
        await enqueuerContext.SaveChangesAsync(cancellationToken);

        return new CreateQueueResponse(queue.Id, queue.Name);
    }

    /// <inheritdoc/>
    /// <exception cref="UserDoesNotExistException" />
    /// <exception cref="QueueDoesNotExistException" />
    /// <exception cref="UserAlreadyParticipatesException" />
    /// <exception cref="QueueIsFullException" />
    public async Task<EnqueueResponse> EnqueueOnFirstAvailablePositionAsync(long userId, int queueId, CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var enqueuerContext = scope.ServiceProvider.GetRequiredService<EnqueuerContext>();

        var user = await enqueuerContext.Users.FindAsync(new object[] { userId }, cancellationToken);
        if (user == null)
        {
            throw new UserDoesNotExistException($"User with the \"{userId}\" ID does not exist.");
        }

        var queue = await enqueuerContext.Queues.Include(q => q.Members)
            .FirstOrDefaultAsync(q => q.Id == queueId, cancellationToken);
        if (queue == null)
        {
            throw new QueueDoesNotExistException($"Queue with the \"{queueId}\" ID does not exist.");
        }

        if (queue.Members.Any(m => m.UserId == userId))
        {
            throw new UserAlreadyParticipatesException($"User with the \"{userId}\" ID already participates in the \"{queue.Name}\" queue.", queue.Name);
        }

        int firstAvailablePosition;
        try
        {
            firstAvailablePosition =
                    (from position in enqueuerContext.Positions
                     join member in enqueuerContext.QueueMembers
                        on new { Position = position.Value, QueueId = queueId } equals new { member.Position, member.QueueId }
                        into joinedMembers
                     from member in joinedMembers.DefaultIfEmpty()
                     where member == null
                     select position.Value).First();
        }
        catch (InvalidOperationException)
        {
            throw new QueueIsFullException($"All possible positions in the \"{queue.Name}\" queue are reserved.", queue.Name);
        }

        queue.Members.Add(new QueueMember
        {
            Position = firstAvailablePosition,
            User = user,
            Queue = queue,
        });

        
        await enqueuerContext.SaveChangesAsync(cancellationToken);
        return new EnqueueResponse(queue, firstAvailablePosition);
    }

    /// <inheritdoc/>
    /// <exception cref="UserDoesNotExistException" />
    /// <exception cref="QueueDoesNotExistException" />
    /// <exception cref="UserAlreadyParticipatesException" />
    /// <exception cref="QueueIsDynamicException" />
    /// <exception cref="QueueIsFullException" />
    /// <exception cref="PositionIsReservedException" />
    public async Task<EnqueueResponse> EnqueueOnPositionAsync(long userId, int queueId, int position, CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var enqueuerContext = scope.ServiceProvider.GetRequiredService<EnqueuerContext>();

        var user = await enqueuerContext.Users.FindAsync(new object[] { userId }, cancellationToken);
        if (user == null)
        {
            throw new UserDoesNotExistException($"User with the \"{userId}\" ID does not exist.");
        }

        var queue = await enqueuerContext.Queues.Include(q => q.Members)
            .FirstOrDefaultAsync(q => q.Id == queueId, cancellationToken);
        if (queue == null)
        {
            throw new QueueDoesNotExistException($"Queue with the \"{queueId}\" ID does not exist.");
        }

        if (queue.Members.Any(m => m.UserId == userId))
        {
            throw new UserAlreadyParticipatesException($"User with the \"{userId}\" ID already participates in the \"{queue.Name}\" queue.", queue.Name);
        }

        if (queue.IsDynamic)
        {
            throw new QueueIsDynamicException($"Enqueing in a specified position is not allowed because queue \"{queue.Name}\" is dynamic.");
        }

        if (queue.Members.Count >= QueueConstants.MaxPosition)
        {
            throw new QueueIsFullException($"All possible positions in the \"{queue.Name}\" queue are reserved.", queue.Name);
        }

        if (queue.Members.Any(m => m.Position == position))
        {
            throw new PositionIsReservedException($"Position \"{position}\" in the \"{queue.Name}\" queue is reserved.", queue.Name, position);
        }

        queue.Members.Add(new QueueMember
        {
            Position = position,
            User = user,
            Queue = queue,
        });

        await _enqueuerContext.SaveChangesAsync(cancellationToken);
        return new EnqueueResponse(queue, position);
    }

    public async Task<Queue> DeleteQueueAsync(int queueId, long userId, bool checkIfCreator, CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var enqueuerContext = scope.ServiceProvider.GetRequiredService<EnqueuerContext>();

        var queue = await enqueuerContext.Queues.FindAsync(new object[] { queueId }, cancellationToken);
        if (queue == null)
        {
            throw new QueueDoesNotExistException($"Queue with the \"{queueId}\" ID does not exist.");
        }

        if (checkIfCreator && queue.CreatorId != userId)
        {
            // Throw
        }

        enqueuerContext.Queues.Remove(queue);
        await enqueuerContext.SaveChangesAsync(cancellationToken);
        return queue;
    }

    public Task<Queue?> GetQueueAsync(int id, bool includeMembers, CancellationToken cancellationToken)
    {
        if (includeMembers)
        {
            return _enqueuerContext.Queues.Include(q => q.Members)
                .ThenInclude(m => m.User)
                .FirstOrDefaultAsync(q => q.Id == id, cancellationToken);
        }

        return _enqueuerContext.Queues.FirstOrDefaultAsync(q => q.Id == id, cancellationToken);
    }

    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException"/>
    public Task AddQueueAsync(Queue queue, CancellationToken cancellationToken)
    {
        if (queue == null)
        {
            throw new ArgumentNullException(nameof(queue));
        }

        _enqueuerContext.Queues.Add(queue);
        return _enqueuerContext.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException"/>
    public Task DeleteQueueAsync(Queue queue, CancellationToken cancellationToken)
    {
        if (queue == null)
        {
            throw new ArgumentNullException(nameof(queue));
        }

        _enqueuerContext.Queues.Remove(queue);
        return _enqueuerContext.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException"/>
    public Task<bool> TryEnqueueUserOnPositionAsync(User user, int queueId, int position, CancellationToken cancellationToken)
    {
        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        return TryAddUserOnPositionAsyncInternal(user, queueId, position, cancellationToken);
    }

    private async Task<bool> TryAddUserOnPositionAsyncInternal(User user, int queueId, int position, CancellationToken cancellationToken)
    {
        var queue = await _enqueuerContext.Queues.Include(q => q.Members)
            .FirstOrDefaultAsync(q => q.Id == queueId, cancellationToken);

        if (queue == null)
        {
            throw new ArgumentException("Queue with the specified ID does not exist.", nameof(queueId));
        }

        if (queue.Members.Any(m => m.Position == position))
        {
            return false;
        }

        queue.Members.Add(new QueueMember
        {
            Position = position,
            UserId = user.Id,
            QueueId = queueId
        });

        await _enqueuerContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException"/>
    public Task<int> AddAtFirstAvailablePosition(User user, int queueId, CancellationToken cancellationToken)
    {
        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        return AddAtFirstAvailablePositionInternal(user, queueId, cancellationToken);
    }

    private async Task<int> AddAtFirstAvailablePositionInternal(User user, int queueId, CancellationToken cancellationToken)
    {
        var queue = await _enqueuerContext.Queues.Include(q => q.Members)
            .FirstOrDefaultAsync(q => q.Id == queueId, cancellationToken);

        if (queue == null)
        {
            throw new ArgumentException("Queue with the specified ID does not exist.", nameof(queueId));
        }
        
        var firstUnreservedPosition = _enqueuerContext.Positions
            .GroupJoin(_enqueuerContext.QueueMembers.Where(qm => qm.QueueId == queueId),
                    p => p.Value,
                    qm => qm.Position,
                    (p, qms) => new { Position = p, QueueMembers = qms })
            .Where(x => !x.QueueMembers.Any())
            .Select(x => x.Position.Value)
            .Min();

        _enqueuerContext.QueueMembers.Add(new QueueMember
        {
            Position = firstUnreservedPosition,
            UserId = user.Id,
            QueueId = queueId
        });

        _enqueuerContext.SaveChanges();

        return firstUnreservedPosition;
    }

    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException"/>
    public Task<bool> TryDequeueUserAsync(User user, int queueId, CancellationToken cancellationToken)
    {
        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        return TryDequeueUserAsyncInternal(user, queueId, cancellationToken);
    }

    private async Task<bool> TryDequeueUserAsyncInternal(User user, int queueId, CancellationToken cancellationToken)
    {
        var queue = await _enqueuerContext.Queues.Include(q => q.Members)
            .FirstOrDefaultAsync(q => q.Id == queueId, cancellationToken);

        if (queue == null)
        {
            throw new ArgumentException("Queue with the specified ID does not exist.", nameof(queueId));
        }

        var userInQueue = queue.Members.FirstOrDefault(m => m.UserId == user.Id);
        if (userInQueue == null)
        {
            return false;
        }

        queue.Members.Remove(userInQueue);
        if (queue.IsDynamic)
        {
            CompressQueuePositions(queue);
        }

        await _enqueuerContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException"/>
    public Task<Queue?> GetQueueByNameAsync(long groupId, string name, bool includeMembers, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentNullException(nameof(name));
        }

        return GetQueueAsyncInternal(groupId, name, includeMembers, cancellationToken);
    }

    private Task<Queue?> GetQueueAsyncInternal(long groupId, string name, bool includeMembers, CancellationToken cancellationToken)
    {
        if (includeMembers)
        {
            return _enqueuerContext.Queues.Include(q => q.Members)
                .ThenInclude(m => m.User)
                .FirstOrDefaultAsync(q => q.GroupId == groupId && q.Name.Equals(name), cancellationToken);
        }

        return _enqueuerContext.Queues.FirstOrDefaultAsync(q => q.GroupId == groupId && q.Name.Equals(name), cancellationToken);
    }

    public Task<List<Queue>> GetGroupQueuesAsync(long groupId, CancellationToken cancellationToken)
    {
        return _enqueuerContext.Queues.Where(q => q.GroupId == groupId).ToListAsync(cancellationToken);
    }

    // TODO: fix
    public async Task SwitchQueueStatusAsync(long queueId, CancellationToken cancellationToken)
    {
        var queue = await _enqueuerContext.Queues.Include(q => q.Members)
            .FirstOrDefaultAsync(q => q.Id == queueId, cancellationToken);

        if (queue == null)
        {   
            return; // TODO: throw exception or return false
        }

        queue.IsDynamic = !queue.IsDynamic;
        if (queue.IsDynamic)
        {
            CompressQueuePositions(queue);
        }

        _enqueuerContext.Update(queue);
        await _enqueuerContext.SaveChangesAsync();
    }

    public async Task SwapMembersPositionsAsync(int queueId, long firstUserId, long secondUserId, CancellationToken cancellationToken)
    {
        var firstMember = await _enqueuerContext.QueueMembers.FindAsync(new object[] { firstUserId, queueId }, cancellationToken);
        var secondMember = await _enqueuerContext.QueueMembers.FindAsync(new object[] { secondUserId, queueId }, cancellationToken);

        if (firstMember != null && secondMember != null)
        {
            // Swap the positions of the two members
            (secondMember.Position, firstMember.Position) = (firstMember.Position, secondMember.Position);

            // Update the database
            _enqueuerContext.Update(firstMember);
            _enqueuerContext.Update(secondMember);
            await _enqueuerContext.SaveChangesAsync(cancellationToken);
        }
    }

    private static void CompressQueuePositions(Queue queue)
    {
        var members = queue.Members.OrderBy(m => m.Position);
        var currentPosition = 1;
        foreach (var member in members)
        {
            if (member.Position != currentPosition)
            {
                member.Position = currentPosition;
            }

            currentPosition++;
        }
    }
}
