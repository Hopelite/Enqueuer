using System.Collections.Generic;

namespace Enqueuer.Persistence.Models;

/// <summary>
/// Represents a queue.
/// </summary>
public class Queue
{
    /// <summary>
    /// Queue ID.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Queue name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// ID of the group to which this queue belongs.
    /// </summary>
    public long GroupId { get; set; }

    /// <summary>
    /// Group or supergroup to which this queue belongs.
    /// </summary>
    public Group Group { get; set; }

    /// <summary>
    /// Optional. Queue creator's ID.
    /// </summary>
    public long? CreatorId { get; set; }

    /// <summary>
    /// Optional. Queue creator.
    /// </summary>
    public User Creator { get; set; }

    /// <summary>
    /// Indicates whether the queue is dynamic or not.
    /// </summary>
    public bool IsDynamic { get; set; }

    /// <summary>
    /// Users participating in this queue.
    /// </summary>
    public ICollection<QueueMember> Members { get; set; }
}
