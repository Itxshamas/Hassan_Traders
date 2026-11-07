using HassanTraders.Desktop.Core.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HassanTraders.Desktop.Data.Persistence.Configurations;

internal sealed class SettingConfiguration : IEntityTypeConfiguration<Setting>
{
    public void Configure(EntityTypeBuilder<Setting> builder)
    {
        builder.ToTable("Settings");
        builder.HasKey(x => x.Key);
        builder.Property(x => x.Key).HasMaxLength(128);
        builder.Property(x => x.Value).HasMaxLength(2048).IsRequired();
    }
}
