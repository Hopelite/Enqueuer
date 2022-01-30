﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Enqueuer.Persistence.Models
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
        [StringLength(64)]
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets user last name.
        /// </summary>
        [StringLength(64)]
        public string LastName { get; set; }

        /// <summary>
        /// Gets or sets Telegram user username.
        /// </summary>
        [StringLength(32)]
        public string UserName { get; set; }

        /// <summary>
        /// Gets or sets chats in which user participates.
        /// </summary>
        public ICollection<Chat> Chats { get; set;} = new List<Chat>();

        /// <summary>
        /// Gets or sets queues which where created by user.
        /// </summary>
        public ICollection<Queue> CreatedQueues { get; set; } = new List<Queue>();

        /// <summary>
        /// Gets or sets queues in which user is registered.
        /// </summary>
        public ICollection<Queue> Queues { get; set; } = new List<Queue>();

        public static implicit operator User(Telegram.Bot.Types.User user)
        {
            return new User()
            {
                UserId = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                UserName = user.Username,
                Chats = new List<Chat>(),
                CreatedQueues = new List<Queue>(),
                Queues = new List<Queue>()
            };
        }
    }
}
