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
        public string Command { get; set; }

        /// <summary>
        /// Gets or sets chat ID.
        /// </summary>
        public long ChatId { get; set; }

        /// <summary>
        /// Optional. Gets or sets callback queue data.
        /// </summary>
        public QueueData QueueData { get; set; }
    }
}
