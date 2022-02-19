using System.Linq;
using Enqueuer.Persistence.Models;

namespace Enqueuer.Messages.Extensions
{
    /// <summary>
    /// Contains extension methods for <see cref="User"/>.
    /// </summary>
    public static class UserExtensions
    {
        /// <summary>
        /// Checks if <paramref name="user"/> participates in <paramref name="queue"/>.
        /// </summary>
        /// <param name="user"><see cref="User"/> to search for in <paramref name="queue"/>.</param>
        /// <param name="queue"><see cref="Queue"/> to check.</param>
        /// <returns>True, if <paramref name="user"/> participates in <paramref name="queue"/>; false otherwise.</returns>
        public static bool IsParticipatingIn(this User user, Queue queue)
        {
            return queue.Users.Any(queueUser => queueUser.UserId == user.Id);
        }
    }
}
