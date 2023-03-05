using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Enqueuer.Persistence.Models;

namespace Enqueuer.Services;

public interface IQueueService
{
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
    /// Gets queues belonging to <see cref="Group"/> with specified <paramref name="chatId"/>.
    /// </summary>
    /// <returns><see cref="IEnumerable{T}"/> of <see cref="Queue"/> belonging to chat.</returns>
    public IEnumerable<Queue> GetChatQueues(int chatId);

    /// <summary>
    /// Gets queues belonging to <see cref="Group"/> with specified <paramref name="chatId"/>.
    /// </summary>
    /// <returns><see cref="IEnumerable{T}"/> of <see cref="Queue"/> belonging to chat.</returns>
    public IEnumerable<Queue> GetTelegramChatQueues(long chatId);

    /// <summary>
    /// Gets <see cref="Queue"/> by <paramref name="name"/> which belongs to <see cref="Group"/> with specified <paramref name="chatId"/>.
    /// </summary>
    /// <returns><see cref="Queue"/> with specified <paramref name="name"/> and <paramref name="chatId"/>; null if doesn't exist.</returns>
    public Queue GetChatQueueByName(string name, long chatId);

    /// <summary>
    /// Gets <see cref="Queue"/> by <paramref name="id"/>.
    /// </summary>
    /// <returns><see cref="Queue"/> with specified <paramref name="id"/>; null if doesn't exist.</returns>
    public Queue GetQueueById(int id);

    /// <summary>
    /// Removes <paramref name="user"/> from <paramref name="queue"/>.
    /// </summary>
    public Task RemoveUserAsync(Queue queue, User user);

    /// <summary>
    /// Updates <paramref name="queue"/>.
    /// </summary>
    public Task UpdateQueueAsync(Queue queue);

    Task AddAsync(Queue queue);
}
