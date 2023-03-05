namespace Enqueuer.Persistence.Models;

/// <summary>
/// Represents a Telegram chat of any type.
/// </summary>
public abstract class Chat
{
    /// <summary>
    /// The Telegram chat ID.
    /// </summary>
    public long Id { get; set; }
}
