using Newtonsoft.Json;

namespace Enqueuer.Data
{
    /// <summary>
    /// Contains callback data.
    /// </summary>
    public class CallbackData
    {
        /// <summary>
        /// Gets or sets the callback command.
        /// </summary>
        [JsonProperty("c")]
        public string Command { get; set; }

        /// <summary>
        /// Gets or sets chat ID.
        /// </summary>
        [JsonProperty("id")]
        public int ChatId { get; set; }

        /// <summary>
        /// Optional. Gets or sets callback queue data.
        /// </summary>
        [JsonProperty("d")]
        public QueueData QueueData { get; set; }
    }
}
