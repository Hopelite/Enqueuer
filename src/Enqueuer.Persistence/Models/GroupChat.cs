using System.Collections.Generic;

namespace Enqueuer.Persistence.Models;

/// <summary>
/// Represents a Telegram group or supergroup.
/// </summary>
public class GroupChat
{    
    /// <summary>
    /// The Telegram chat ID.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// The Telegram group title.
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// Users participating in this group.
    /// </summary>
    public ICollection<User> Members { get; set; }

    /// <summary>
    /// Queues created for this group.
    /// </summary>
    public ICollection<Queue> Queues { get; set; }
}
