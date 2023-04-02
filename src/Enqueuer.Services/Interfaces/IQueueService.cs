using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Enqueuer.Persistence.Models;
using Enqueuer.Services.Responses;

namespace Enqueuer.Services;

public interface IQueueService
{
    /// <summary>
    /// Creates a queue with the specified <paramref name="queueName"/> in a chat with the specified <paramref name="groupId"/>
    /// on behalf of user with the <paramref name="creatorId"/> ID. If <paramref name="position"/> is specified, enqueues creator on it.
    /// </summary>
    Task<CreateQueueResponse> CreateQueueAsync(long creatorId, long groupId, string queueName, int? position = null, CancellationToken cancellationToken = default);
        
    /// <summary>
    /// Adds user with the specified <paramref name="userId"/> on the first available position in a queue with the <paramref name="queueId"/>.
    /// </summary>
    Task<EnqueueResponse> EnqueueOnFirstAvailablePositionAsync(long userId, int queueId, CancellationToken cancellationToken);

    /// <summary>
    /// Adds user with the specified <paramref name="userId"/> on the specified <paramref name="position"/> in a queue with the <paramref name="queueId"/>.
    /// </summary>
    Task<EnqueueResponse> EnqueueOnPositionAsync(long userId, int queueId, int position, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes <see cref="Queue"/> with the specified <paramref name="queueId"/> 
    /// checking whether user with <paramref name="userId"/> has rights to delete it if <paramref name="checkIfCreator"/> is checked.
    /// </summary>
    Task<Queue> DeleteQueueAsync(int queueId, long userId, bool checkIfCreator, CancellationToken cancellationToken);

    /// <summary>
    /// Gets <see cref="Queue"/> with the specified <paramref name="id"/>.
    /// </summary>
    Task<Queue?> GetQueueAsync(int id, bool includeMembers, CancellationToken cancellationToken);

    /// <summary>
    /// Adds the <paramref name="queue"/>.
    /// </summary>
    Task AddQueueAsync(Queue queue, CancellationToken cancellationToken);

    /// <summary>
    /// Tries to add the <paramref name="queue"/> if it doesn't exist salready.
    /// </summary>
    Task<bool> TryAddQueueAsync(Queue queue, CancellationToken cancellationToken);

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

    /// <summary>
    /// Swaps the <paramref name="firstUserId"/>'s position with the <paramref name="secondUserId"/>'s one
    /// in a queue with the specified <paramref name="queueId"/>.
    /// </summary>
    Task SwapMembersPositionsAsync(int queueId, long firstUserId, long secondUserId, CancellationToken cancellationToken);
}
