using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Enqueuer.Data.Models
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
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets chat ID to which this queue belongs to.
        /// </summary>
        public int ChatId { get; set; }

        /// <summary>
        /// Gets or sets chat to which this queue belongs to.
        /// </summary>
        [ForeignKey(nameof(ChatId))]
        public Chat Chat { get; set; }

        /// <summary>
        /// Gets or sets this queue creator ID.
        /// </summary>
        public int CreatorId { get; set; }

        /// <summary>
        /// Gets or sets this queue creator.
        /// </summary>
        [ForeignKey(nameof(CreatorId))]
        public User Creator { get; set; }
    }
}
