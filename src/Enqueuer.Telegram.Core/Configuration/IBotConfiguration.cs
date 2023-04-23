namespace Enqueuer.Telegram.Core.Configuration;

public interface IBotConfiguration
{
    /// <summary>
    /// Telegram bot access token.
    /// </summary>
    public string AccessToken { get; }

    /// <summary>
    /// Maximum number of queues per group.
    /// </summary>
    public int QueuesPerChat { get; }
}
