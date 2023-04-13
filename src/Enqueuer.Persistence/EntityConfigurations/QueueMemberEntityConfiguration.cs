using Enqueuer.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Enqueuer.Persistence.EntityConfigurations;

internal class QueueMemberEntityConfiguration : IEntityTypeConfiguration<QueueMember>
{
    public void Configure(EntityTypeBuilder<QueueMember> builder)
    {
        builder.HasKey(member => new { member.UserId, member.QueueId });

        builder.HasOne(queueMember => queueMember.User)
            .WithMany(u => u.ParticipatesIn)
            .HasForeignKey(m => m.UserId);

        builder.HasOne(queueMember => queueMember.Queue)
            .WithMany(q => q.Members)
            .HasForeignKey(queueMember => queueMember.QueueId);
    }
}
