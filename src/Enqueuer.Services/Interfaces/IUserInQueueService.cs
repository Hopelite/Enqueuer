using Enqueuer.Persistence.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Enqueuer.Services;

/// <summary>
/// Contains methods for <see cref="QueueMember"/>.
/// </summary>
public interface IUserInQueueService
{
    /// <summary>
    /// Gets first available position in queue.
    /// </summary>
    /// <param name="queue"><see cref="Queue"/> where to search.</param>
    /// <returns>First available position <paramref name="queue"/>.</returns>
    public int GetFirstAvailablePosition(Queue queue);

    /// <summary>
    /// Checks whether specified <paramref name="position"/> in this <paramref name="queue"/> already reserved.
    /// </summary>
    /// <param name="queue"><see cref="Queue"/> which positions to check.</param>
    /// <param name="position">Position to check.</param>
    /// <returns>True, if <paramref name="position"/> is reserved; false otherwise.</returns>
    public bool IsPositionReserved(Queue queue, int position);

    /// <summary>
    /// Returns available positions in queue.
    /// </summary>
    /// <param name="queue">Queue, which positions to check.</param>
    /// <returns>Available positions.</returns>
    public IEnumerable<int> GetAvailablePositions(Queue queue);

    /// <summary>
    /// Adds <paramref name="user"/> to <paramref name="queue"/> at the specified <paramref name="position"/>.
    /// </summary>
    /// <param name="user"><see cref="User"/> to add.</param>
    /// <param name="queue"><see cref="Queue"/> to add into.</param>
    /// <param name="position">Position to add <paramref name="user"/> at.</param>
    public Task AddUserToQueueAsync(User user, Queue queue, int position);

    /// <summary>
    /// Compresses all positions in <paramref name="queue"/> starting from <paramref name="startingAtPosition"/>.
    /// </summary>
    public Task CompressQueuePositionsAsync(Queue queue, int startingAtPosition = 1);
}
