namespace Enqueuer.Data.Configuration;

public interface IBotConfiguration
{
    /// <summary>
    /// Telegram bot access token.
    /// </summary>
    public string AccessToken { get; }

    /// <summary>
    /// The maximum number of queues per group.
    /// </summary>
    public int QueuesPerChat { get; }
}
