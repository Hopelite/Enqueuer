using Enqueuer.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Enqueuer.Persistence.EntityConfigurations;

internal class BlocklistRecordEntityConfiguration : IEntityTypeConfiguration<BlocklistRecord>
{
    public void Configure(EntityTypeBuilder<BlocklistRecord> builder)
    {
        //builder.HasKey(record => new { record.UserId, record.BlockedUserId, record.QueueId });

        //builder.HasOne(record => record.User)
        //    .WithMany(user => user.BlockedUsers)
        //    .HasForeignKey(record => record.UserId);
    }
}
