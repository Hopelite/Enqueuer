using Enqueuer.Persistence.Constants;
using Enqueuer.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Enqueuer.Persistence.EntityConfigurations;

internal class QueueEntityConfiguration : IEntityTypeConfiguration<Queue>
{
    public void Configure(EntityTypeBuilder<Queue> builder)
    {
        builder.HasKey(q => new { q.GroupId, q.Id });

        builder.Property(u => u.Name)
            .HasMaxLength(QueueConstants.MaxNameLength)
            .IsRequired();
    }
}
