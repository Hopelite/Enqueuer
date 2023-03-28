using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Enqueuer.Persistence;
using Enqueuer.Persistence.Models;
using Microsoft.EntityFrameworkCore;

namespace Enqueuer.Services;

public class QueueService : IQueueService
{
    private readonly EnqueuerContext _enqueuerContext;

    public QueueService(EnqueuerContext enqueuerContext)
    {
        _enqueuerContext = enqueuerContext;
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

        var position = GetFirstAvailablePosition();
        queue.Members.Add(new QueueMember
        {
            Position = position,
            UserId = user.Id,
            QueueId = queueId
        });

        await _enqueuerContext.SaveChangesAsync(cancellationToken);
        return position;

        int GetFirstAvailablePosition()
        {
            var firstAvailablePosition = 1;
            var positions = queue.Members.OrderBy(m => m.Position).Select(m => m.Position);
            foreach (var position in positions)
            {
                if (position != firstAvailablePosition)
                {
                    return firstAvailablePosition;
                }

                firstAvailablePosition++;
            }

            return firstAvailablePosition;
        }
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
