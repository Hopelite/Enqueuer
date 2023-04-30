namespace Enqueuer.Service.Messages.Models
{
    /// <summary>
    /// A detailed representation of a Telegram group or supergroup.
    /// </summary>
    public class GroupInfo : Group
    {
        /// <summary>
        /// Queues in this group.
        /// </summary>
        public Queue[] Queues { get; set; }
    }
}
