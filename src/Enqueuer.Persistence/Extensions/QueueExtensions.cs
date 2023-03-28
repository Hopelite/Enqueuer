using Enqueuer.Persistence.Models;
using System.Linq;

namespace Enqueuer.Persistence.Extensions
{
    /// <summary>
    /// Contains extension methods for <see cref="Queue"/>.
    /// </summary>
    public static class QueueExtensions
    {
        /// <summary>
        /// Checks whether <paramref name="user"/> is the <paramref name="queue"/> creator.
        /// </summary>
        /// <param name="queue"><see cref="Queue"/> which creator to check.</param>
        /// <param name="user">Telegram user to check.</param>
        /// <returns>True, if <paramref name="user"/> is <paramref name="queue"/> creator.</returns>
        public static bool IsQueueCreator(this Queue queue, User user)
        {
            return queue.IsQueueCreator(user.Id);
        }

        /// <summary>
        /// Checks whether user with specified <paramref name="userId"/> is <paramref name="queue"/> creator.
        /// </summary>
        /// <param name="queue"><see cref="Queue"/> which creator to check.</param>
        /// <param name="userId">Telegram user ID to check creator by.</param>
        /// <returns>True, if user with specified <paramref name="userId"/> is <paramref name="queue"/> creator.</returns>
        public static bool IsQueueCreator(this Queue queue, long userId)
        {
            return queue.CreatorId == userId;
        }
    }
}
