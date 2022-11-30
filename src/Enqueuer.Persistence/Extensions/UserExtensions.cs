using System.Linq;
using Enqueuer.Persistence.Models;

namespace Enqueuer.Persistence.Extensions
{
    /// <summary>
    /// Contains extension methods for <see cref="User"/>.
    /// </summary>
    public static class UserExtensions
    {
        /// <summary>
        /// Checks if <paramref name="user"/> participates in <paramref name="queue"/>.
        /// </summary>
        /// <returns>True, if <paramref name="user"/> participates in <paramref name="queue"/>; false otherwise.</returns>
        public static bool IsParticipatingIn(this User user, Queue queue)
        {
            return queue.Users.Any(queueUser => queueUser.UserId == user.Id);
        }

        /// <summary>
        /// Tries to get <paramref name="user"/> position in <paramref name="queue"/>.
        /// </summary>
        /// <returns>True, if <paramref name="user"/> participates in <paramref name="queue"/>; false otherwise.</returns>
        public static bool TryGetUserPosition(this User user, Queue queue, out int position)
        {
            position = -1;
            var userInQueue = queue.Users.FirstOrDefault(queueUser => queueUser.UserId == user.Id);
            if (userInQueue == null)
            {
                return false;
            }

            position = userInQueue.Position;
            return true;
        }
    }
}
