using Enqueuer.Persistence.Models;

namespace Enqueuer.Services.Interfaces
{
    /// <summary>
    /// Contains methods for <see cref="UserInQueue"/>.
    /// </summary>
    public interface IUserInQueueService
    {
        /// <summary>
        /// Gets total number of users in queue.
        /// </summary>
        /// <param name="queue"><see cref="Queue"/> where to search.</param>
        /// <returns>Number of users in <see cref="Queue"/>.</returns>
        public int GetTotalUsersInQueue(Queue queue);
    }
}
