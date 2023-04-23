using Enqueuer.Telegram.Core.Configuration;

namespace Enqueuer.Telegram.Configuration;

public class BotConfiguration : IBotConfiguration
{
    public string AccessToken { get; set; }

    public int QueuesPerChat { get; set; }
}
