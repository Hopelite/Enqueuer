namespace Enqueuer.Telegram.Sessions.Types;

/// <summary>
/// Represents an active Telegram chat session.
/// </summary>
public class Session
{
    /// <summary>
    /// Telegram chat ID the session is related to.
    /// Can be either group ID or a user ID.
    /// </summary>
    public long ChatId { get; set; }

    /// <summary>
    /// The last command that was executed in this chat.
    /// </summary>
    public CommandContext? LastCommand { get; set; }
}
