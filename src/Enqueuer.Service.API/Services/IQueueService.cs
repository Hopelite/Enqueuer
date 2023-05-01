using System.Threading.Tasks;
using System.Threading;
using Enqueuer.Service.Messages.Models;
using Enqueuer.Service.Messages;
using Enqueuer.Service.API.Services.Exceptions;

namespace Enqueuer.Service.API.Services;

public interface IQueueService
{
    /// <summary>
    /// Gets the <see cref="QueueInfo"/> of an existing queue with the specified <paramref name="queueId"/> if exists.
    /// </summary>
    Task<QueueInfo?> GetQueueAsync(int queueId, CancellationToken cancellationToken);

    /// <summary>
    /// Creates a queue with the specified queue name in a chat with the specified group ID on behalf of user with the creator ID.
    /// </summary>
    /// <returns>The <see cref="QueueInfo"/> of the created queue.</returns>
    /// <exception cref="QueueAlreadyExistsException" />
    Task<QueueInfo> CreateQueueAsync(CreateQueueRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Get a participant info with the specified <paramref name="userId"/> which participates in a queue with the specified <paramref name="queueId"/>.
    /// </summary>
    Task<QueueMember?> GetQueueMemberAsync(int queueId, long userId, CancellationToken cancellationToken);

    /// <summary>
    /// Adds <paramref name="user"/> to a queue with the <paramref name="queueId"/>.
    /// on the specified <paramref name="position"/> in
    /// </summary>
    Task<int> EnqueueUserAsync(int queueId, User user, int? position, CancellationToken cancellationToken);
}
