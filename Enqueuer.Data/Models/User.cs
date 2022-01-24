using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Enqueuer.Data.Models
{
    /// <summary>
    /// Represents Telegram user.
    /// </summary>
    public class User
    {
        /// <summary>
        /// Gets or sets user ID.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets Telegram user ID.
        /// </summary>
        [Required]
        public long UserId { get; set; }

        /// <summary>
        /// Gets or sets user first name.
        /// </summary>
        [Required]
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets user last name.
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// Gets or sets Telegram user username.
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Gets or sets chats in which user participates.
        /// </summary>
        public virtual ICollection<Chat> Chats { get; set;}

        /// <summary>
        /// Gets or sets queues in which user is registered.
        /// </summary>
        public virtual ICollection<Queue> Queues { get;}
    }
}
