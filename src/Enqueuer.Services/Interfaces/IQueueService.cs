using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Enqueuer.Persistence.Models;

namespace Enqueuer.Services;

public interface IQueueService
{
    /// <summary>
    /// Gets <see cref="Queue"/> with the specified <paramref name="id"/>.
    /// </summary>
    Task<Queue?> GetQueueAsync(int id, bool includeMembers, CancellationToken cancellationToken);

    /// <summary>
    /// Adds the <paramref name="queue"/>.
    /// </summary>
    Task AddQueueAsync(Queue queue, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes the <paramref name="queue"/>.
    /// </summary>
    Task DeleteQueueAsync(Queue queue, CancellationToken cancellationToken);

    /// <summary>
    /// Tries to add <paramref name="user"/> to the <see cref="Queue"/> with the specified <paramref name="queueId"/> at the specified <paramref name="position"/>.
    /// </summary>
    Task<bool> TryEnqueueUserOnPositionAsync(User user, int queueId, int position, CancellationToken cancellationToken);

    /// <summary>
    /// Adds <paramref name="user"/> to the <see cref="Queue"/> with the specified <paramref name="queueId"/> at the first available position.
    /// </summary>
    /// <returns>The position to which the <paramref name="user"/> was added.</returns>
    Task<int> AddAtFirstAvailablePosition(User user, int queueId, CancellationToken cancellationToken);

    /// <summary>
    /// Tries to remove <paramref name="user"/> from the <see cref="Queue"/> with the specified <paramref name="queueId"/>.
    /// </summary>
    Task<bool> TryDequeueUserAsync(User user, int queueId, CancellationToken cancellationToken);

    /// <summary>
    /// Gets <see cref="Queue"/> by <paramref name="name"/> which belongs to a <see cref="Group"/> with the specified <paramref name="groupId"/>.
    /// </summary>
    Task<Queue?> GetQueueByNameAsync(long groupId, string name, bool includeMembers, CancellationToken cancellationToken);

    /// <summary>
    /// Gets all <see cref="Queue"/>s that exist in the <see cref="Group"/> with the specified <paramref name="groupId"/>.
    /// </summary>
    Task<List<Queue>> GetGroupQueuesAsync(long groupId, CancellationToken cancellationToken);

    /// <summary>
    /// Changes the queue's with the specified <paramref name="queueId"/> status to the opposite one.
    /// </summary>
    Task SwitchQueueStatusAsync(long queueId, CancellationToken cancellationToken);
}
