namespace Enqueuer.Persistence.Models;

/// <summary>
/// Represents a user in queue.
/// </summary>
public class QueueMember
{
    /// <summary>
    /// User's position in the queue.
    /// </summary>
    public int Position { get; set; }

    /// <summary>
    /// Queue member ID.
    /// </summary>
    public long UserId { get; set; }

    /// <summary>
    /// Queue member.
    /// </summary>
    public User User { get; set; }

    /// <summary>
    /// ID of the queue in which user participates.
    /// </summary>
    public int QueueId { get; set; }

    /// <summary>
    /// Queue in which user participates.
    /// </summary>
    public Queue Queue { get; set; }

    /// <summary>
    /// ID of the group to which queue is related.
    /// </summary>
    public long GroupId { get; set; }

    /// <summary>
    /// Group related to the queue.
    /// </summary>
    public Group Group { get; set; }
}
