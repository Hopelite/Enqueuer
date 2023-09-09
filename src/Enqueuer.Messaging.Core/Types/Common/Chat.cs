namespace Enqueuer.Messaging.Core.Types.Common;

/// <summary>
/// Represents a Telegram chat of any type.
/// </summary>
public abstract class Chat
{
    /// <summary>
    /// Unique identifier of this chat.
    /// </summary>
    public long Id { get; set; }
}
