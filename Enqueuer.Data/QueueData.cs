using Newtonsoft.Json;

namespace Enqueuer.Data
{
    /// <summary>
    /// Contains queue data for callback.
    /// </summary>
    public class QueueData
    {
        /// <summary>
        /// Gets or sets queue ID.
        /// </summary>
        [JsonProperty("qi")]
        public int QueueId { get; set; }

        /// <summary>
        /// Optional. Gets or sets user position in queue.
        /// </summary>
        [JsonProperty("p")]
        public int? Position { get; set; }

        /// <summary>
        /// Optional. Gets or sets value indicating whether user is agreed to delete a queue.
        /// </summary>
        [JsonProperty("a")]
        public bool? IsUserAgreed { get; set; }
    }
}
