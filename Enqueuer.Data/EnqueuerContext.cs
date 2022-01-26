using Enqueuer.Persistence.Models;
using Microsoft.EntityFrameworkCore;

namespace Enqueuer.Persistence 
{
    /// <summary>
    /// A <see cref="DbContext"/> to work with Enqueuer Bot entities.
    /// </summary>
    public class EnqueuerContext : DbContext
    {
        /// <summary>
        /// Gets or sets queryable set of <see cref="Chat"/> entities.
        /// </summary>
        public DbSet<Chat> Chats { get; set; }

        /// <summary>
        /// Gets or sets queryable set of <see cref="User"/> entities.
        /// </summary>
        public DbSet<User> Users { get; set; }

        /// <summary>
        /// Gets or sets queryable set of <see cref="Queue"/> entities.
        /// </summary>
        public DbSet<Queue> Queues { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="EnqueuerContext"/> class.
        /// </summary>
        /// <param name="dbContextOptions">The options for <see cref="EnqueuerContext"/>.</param>
        public EnqueuerContext(DbContextOptions<EnqueuerContext> dbContextOptions)
            : base(dbContextOptions)
        {
            Database.EnsureCreated();
        }

        ///  <inheritdoc/>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Chat>().HasIndex(chat => chat.ChatId)
                .IsUnique();
            modelBuilder.Entity<User>().HasIndex(user => user.UserId)
                .IsUnique();
            modelBuilder.Entity<User>().HasMany(user => user.QueuesCreated)
                .WithOne(queue => queue.Creator);
            modelBuilder.Entity<User>().HasMany(user => user.Queues)
                .WithMany(queue => queue.Users);
        }
    }
}