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
        public int QueueId { get; set; }

        /// <summary>
        /// Optional. Gets or sets user position in queue.
        /// </summary>
        public int? Position { get; set; }

        /// <summary>
        /// Optional. Gets or sets value indicating whether user is agreed to delete a queue.
        /// </summary>
        public bool? IsUserAgreed { get; set; }
    }
}
