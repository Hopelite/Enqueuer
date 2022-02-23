using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Enqueuer.Persistence.Models
{
    /// <summary>
    /// Represents Telegram chat.
    /// </summary>
    public class Chat
    {
        /// <summary>
        /// Gets or sets chat ID.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets Telegram chat ID.
        /// </summary>
        [Required]
        public long ChatId { get; set; }

        /// <summary>
        /// Gets or sets chat name.
        /// </summary>
        [Required]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets users participating in this chat.
        /// </summary>
        public virtual ICollection<User> Users { get; set; } = new List<User>();

        /// <summary>
        /// Gets or sets queues related to this chat.
        /// </summary>
        public virtual ICollection<Queue> Queues { get; set; } = new List<Queue>();

        public static implicit operator Chat(Telegram.Bot.Types.Chat chat)
        {
            return new Chat() 
            { 
                ChatId = chat.Id,
                Name = chat.Title
            };
        }
    }
}
