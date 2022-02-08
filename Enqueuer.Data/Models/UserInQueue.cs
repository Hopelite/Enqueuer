using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Enqueuer.Persistence.Models
{
    /// <summary>
    /// Represents user in queue.
    /// </summary>
    public class UserInQueue
    {
        /// <summary>
        /// Gets or sets user in queue ID.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets user position in queue.
        /// </summary>
        public int Position { get; set; }

        /// <summary>
        /// Gets or sets user ID.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Gets or sets queue ID.
        /// </summary>
        public int QueueId { get; set; }

        /// <summary>
        /// Gets or sets user.
        /// </summary>
        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        /// <summary>
        /// Gets or sets queue.
        /// </summary>
        [ForeignKey("QueueId")]
        public virtual Queue Queue { get; set; }
    }
}
