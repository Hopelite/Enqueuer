using Enqueuer.Persistence.EntityConfigurations;
using Enqueuer.Persistence.Models;
using Microsoft.EntityFrameworkCore;

namespace Enqueuer.Persistence;

/// <summary>
/// A <see cref="DbContext"/> to work with Enqueuer Bot entities.
/// </summary>
public class EnqueuerContext : DbContext
{
    /// <summary>
    /// Telegram groups and supergroups known to the bot.
    /// </summary>
    public DbSet<Group> Groups { get; set; }

    /// <summary>
    /// Telegram users known to the bot.
    /// </summary>
    public DbSet<User> Users { get; set; }

    /// <summary>
    /// Created queues.
    /// </summary>
    public DbSet<Queue> Queues { get; set; }

    public EnqueuerContext(DbContextOptions<EnqueuerContext> dbContextOptions)
        : base(dbContextOptions)
    {
        Database.EnsureCreated();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .ApplyConfiguration(new GroupChatEntityConfiguration())
            .ApplyConfiguration(new UserEntityConfiguration())
            .ApplyConfiguration(new QueueEntityConfiguration())
            .ApplyConfiguration(new QueueMemberEntityConfiguration());

        base.OnModelCreating(modelBuilder);
    }
}
