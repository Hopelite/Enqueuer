namespace Enqueuer.Persistence.Models;

/// <summary>
/// Represents a record about blocked user.
/// </summary>
public class BlocklistRecord
{
    /// <summary>
    /// Id of the user who blocks.
    /// </summary>
    public long UserId { get; set; }

    /// <summary>
    /// User who blocks.
    /// </summary>
    public User User { get; set; }

    /// <summary>
    /// Blocked user ID.
    /// </summary>
    public long BlockedUserId { get; set; }

    /// <summary>
    /// Blocked user.
    /// </summary>
    public User BlockedUser { get; set; }

    /// <summary>
    /// ID of the queue for which user is blocking requests from the blocked user.
    /// </summary>
    public int QueueId { get; set; }

    /// <summary>
    /// Queue for which user is blocking requests from the blocked user.
    /// </summary>
    public Queue Queue { get; set; }
}
