using System.Collections.Generic;

namespace Enqueuer.Persistence.Models;

/// <summary>
/// Represents a Telegram user.
/// </summary>
public class User
{
    /// <summary>
    /// Telegram user ID.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// User's first name.
    /// </summary>
    public string FirstName { get; set; }

    /// <summary>
    /// Optional. User's last name.
    /// </summary>
    public string LastName { get; set; }

    /// <summary>
    /// Full user's name.
    /// </summary>
    public string FullName => string.IsNullOrWhiteSpace(LastName) ? FirstName : $"{FirstName} {LastName}";

    /// <summary>
    /// Chats in which user participates.
    /// </summary>
    public ICollection<Chat> Chats { get; set; }

    /// <summary>
    /// Queues created by user.
    /// </summary>
    public ICollection<Queue> CreatedQueues { get; set; }

    /// <summary>
    /// Queues in which user participates.
    /// </summary>
    public ICollection<QueueMember> ParticipatesIn { get; set; }
}
