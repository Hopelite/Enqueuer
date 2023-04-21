using Newtonsoft.Json;

namespace Enqueuer.Telegram.Core;

/// <summary>
/// Contains callback data.
/// </summary>
public class CallbackData
{
    /// <summary>
    /// Callback command.
    /// </summary>
    [JsonProperty("c")]
    public string Command { get; set; }

    /// <summary>
    /// Optional. The ID of the Telegram chat targeted by this callback.
    /// </summary>
    [JsonProperty("i")]
    public long? TargetChatId { get; set; }

    /// <summary>
    /// Optional. Page number.
    /// </summary>
    [JsonProperty("p")]
    public int? Page { get; set; }

    /// <summary>
    /// Optional. The ID of the Telegram user targeted by this callback.
    /// </summary>
    [JsonProperty("u")]
    public long? TargetUserId { get; set; }

    /// <summary>
    /// Optional. Queue-related data.
    /// </summary>
    [JsonProperty("d")]
    public QueueData? QueueData { get; set; }

    /// <summary>
    /// Optional. Whether the user has given their explicit consent or not.
    /// </summary>
    [JsonProperty("a")]
    public bool? UserAgreement { get; set; }

    /// <summary>
    /// Whether the user has given their explicit consent or not.
    /// </summary>
    [JsonIgnore]
    public bool HasUserAgreement => UserAgreement.HasValue && UserAgreement.Value;
}
