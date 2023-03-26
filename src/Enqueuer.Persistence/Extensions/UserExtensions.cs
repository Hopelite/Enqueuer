using System;
using System.Linq;
using Enqueuer.Persistence.Models;

namespace Enqueuer.Persistence.Extensions;

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
        if (queue?.Members == null)
        {
            throw new ArgumentNullException(nameof(queue), "Queue Members are null.");
        }

        return queue.Members.Any(queueUser => queueUser.UserId == user.Id);
    }
}
