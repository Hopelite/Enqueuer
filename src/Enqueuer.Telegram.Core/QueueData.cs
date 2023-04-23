using Newtonsoft.Json;

namespace Enqueuer.Telegram.Core;

/// <summary>
/// Contains queue-related callback data.
/// </summary>
public class QueueData
{
    /// <summary>
    /// Queue ID.
    /// </summary>
    [JsonProperty("i")]
    public int QueueId { get; set; }

    /// <summary>
    /// Optional. Queue position.
    /// </summary>
    [JsonProperty("p")]
    public int? Position { get; set; }
}
