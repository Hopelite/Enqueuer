using Enqueuer.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Enqueuer.Persistence.EntityConfigurations;

internal class GroupSettingsEntityConfiguration : IEntityTypeConfiguration<GroupSettings>
{
    public void Configure(EntityTypeBuilder<GroupSettings> builder)
    {
        builder.Ignore(s => s.Culture);
    }
}
