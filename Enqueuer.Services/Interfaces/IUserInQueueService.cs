using Enqueuer.Persistence.Models;
using System.Collections.Generic;

namespace Enqueuer.Services.Interfaces
{
    /// <summary>
    /// Contains methods for <see cref="UserInQueue"/>.
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
    }
}
