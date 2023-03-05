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
        await _enqueuerContext.SaveChangesAsync(cancellationToken);
        return true;
    }



    public Queue GetChatQueueByName(string name, long chatId)
    {
        throw new NotImplementedException();
        //return _queueRepository.GetAll()
        //    .FirstOrDefault(queue => queue.Chat.Id == chatId && queue.Name.Equals(name));
    }

    public IEnumerable<Queue> GetChatQueues(int chatId)
    {
        throw new NotImplementedException();
        //return _queueRepository.GetAll()
        //    .Where(queue => queue.ChatId == chatId);
    }

    public IEnumerable<Queue> GetTelegramChatQueues(long chatId)
    {
        throw new NotImplementedException();
        //return _queueRepository.GetAll()
        //    .Where(queue => queue.Chat.Id == chatId);
    }

    public Queue GetQueueById(int id)
    {
        throw new NotImplementedException();
        //return _queueRepository.Get(id);
    }

    public async Task RemoveUserAsync(Queue queue, User user)
    {
        throw new NotImplementedException();
        //var userToRemove = queue.Members.FirstOrDefault(queueUser => queueUser.UserId == user.Id);
        //if (userToRemove is not null)
        //{
        //    queue.Members.Remove(userToRemove);
        //    await _queueRepository.UpdateAsync(queue);
        //}
    }

    public async Task UpdateQueueAsync(Queue queue)
    {
        throw new NotImplementedException();
        //await _queueRepository.UpdateAsync(queue);
    }

    public Task AddAsync(Queue queue)
    {
        throw new NotImplementedException();
        //return _queueRepository.AddAsync(queue);
    }
}
