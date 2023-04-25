namespace Enqueuer.Service.Messages.Models
{
    /// <summary>
    /// Represents a Telegram group or supergroup.
    /// </summary>
    public class Group
    {
        /// <summary>
        /// Telegram group ID.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Telegram group title.
        /// </summary>
        public string Title { get; set; }
    }
}

