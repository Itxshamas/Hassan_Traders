using HassanTraders.Desktop.Core.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HassanTraders.Desktop.Data.Persistence.Configurations;

internal sealed class AuditEntryConfiguration : IEntityTypeConfiguration<AuditEntry>
{
    public void Configure(EntityTypeBuilder<AuditEntry> builder)
    {
        builder.ToTable("AuditLog");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Action).HasConversion<string>().HasMaxLength(32);
        builder.Property(x => x.Username).HasMaxLength(64);
        builder.Property(x => x.Details).HasMaxLength(1024);
        builder.HasIndex(x => x.TimestampUtc);
        builder.HasIndex(x => x.UserId);
    }
}
