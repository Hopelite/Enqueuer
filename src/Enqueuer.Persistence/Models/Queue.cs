using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Enqueuer.Persistence.Models
{
    /// <summary>
    /// Represents queue.
    /// </summary>
    public class Queue
    {
        /// <summary>
        /// Gets or sets queue ID.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets queue name.
        /// </summary>
        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets chat ID to which this queue belongs to.
        /// </summary>
        public int? ChatId { get; set; }

        /// <summary>
        /// Gets or sets chat to which this queue belongs to.
        /// </summary>
        [ForeignKey("ChatId")]
        public virtual Chat Chat { get; set; }

        /// <summary>
        /// Gets or sets this queue creator ID.
        /// </summary>
        public int? CreatorId { get; set; }

        /// <summary>
        /// Gets or sets this queue creator.
        /// </summary>
        [ForeignKey("CreatorId")]
        public virtual User Creator { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether queue is dynamic or not.
        /// </summary>
        public bool IsDynamic { get; set; }

        /// <summary>
        /// Gets or sets users in this queue.
        /// </summary>
        public virtual ICollection<UserInQueue> Users { get; set; } = new List<UserInQueue>();
    }
}
