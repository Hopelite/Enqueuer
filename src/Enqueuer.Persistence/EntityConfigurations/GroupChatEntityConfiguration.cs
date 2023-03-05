using Enqueuer.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Enqueuer.Persistence.EntityConfigurations;

internal class GroupChatEntityConfiguration : IEntityTypeConfiguration<GroupChat>
{
    public void Configure(EntityTypeBuilder<GroupChat> builder)
    {
        builder.Property(c => c.Title)
            .HasMaxLength(128)
            .IsRequired();
    }
}
