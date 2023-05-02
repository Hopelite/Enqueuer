using System.Threading.Tasks;
using System.Threading;
using Enqueuer.Service.Messages.Models;
using Enqueuer.Service.Messages;
using Enqueuer.Service.API.Services.Exceptions;

namespace Enqueuer.Service.API.Services;

public interface IQueueService
{
    /// <summary>
    /// Gets the <see cref="QueueInfo"/> of an existing queue with the specified <paramref name="queueId"/>
    /// if it exists in a group with the specified <paramref name="groupId"/>.
    /// </summary>
    Task<QueueInfo?> GetQueueAsync(long groupId, int queueId, CancellationToken cancellationToken);

    /// <summary>
    /// Creates a queue with the specified queue name in a chat with the specified <paramref name="groupId"/> on behalf of user with the creator ID.
    /// </summary>
    /// <returns>The <see cref="QueueInfo"/> of the created queue.</returns>
    /// <exception cref="UserDoesNotExistException" />
    /// <exception cref="GroupDoesNotExistException" />
    /// <exception cref="QueueAlreadyExistsException" />
    Task<QueueInfo> CreateQueueAsync(long groupId, CreateQueueRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes a queue with the specified <paramref name="queueId"/>.
    /// </summary>
    /// <exception cref="QueueDoesNotExistException" />
    Task DeleteQueueAsync(long groupId, int queueId, CancellationToken cancellationToken);

    /// <summary>
    /// Get a participant info with the specified <paramref name="userId"/> which participates in a queue with the specified <paramref name="queueId"/>
    /// in a group with the specified <paramref name="groupId"/>.
    /// </summary>
    Task<QueueMember?> GetQueueMemberAsync(long groupId, int queueId, long userId, CancellationToken cancellationToken);

    /// <summary>
    /// Adds user to a queue with the <paramref name="queueId"/>.
    /// If <paramref name="position"/> is specified, add user on it; otherwise adds on the first available position.
    /// </summary>
    /// <exception cref="QueueDoesNotExistException" />
    /// <exception cref="UserAlreadyParticipatesException" />
    Task<int> EnqueueUserAsync(long groupId, int queueId, EnqueueUserRequest request, int? position, CancellationToken cancellationToken);

    /// <summary>
    /// Removes a user with the specified <paramref name="userId"/> to a queue with the <paramref name="queueId"/>.
    /// </summary>
    /// <exception cref="QueueDoesNotExistException" />
    /// <exception cref="UserDoesNotParticipateException" />
    Task DequeueUserAsync(long groupId, int queueId, long userId, CancellationToken cancellationToken);
}
