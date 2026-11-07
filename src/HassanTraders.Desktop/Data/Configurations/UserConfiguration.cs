using HassanTraders.Desktop.Core.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HassanTraders.Desktop.Data.Persistence.Configurations;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Username).HasMaxLength(64).IsRequired();
        builder.Property(x => x.DisplayName).HasMaxLength(128).IsRequired();
        builder.Property(x => x.PasswordHash).HasMaxLength(256).IsRequired();
        builder.Property(x => x.Role).HasConversion<string>().HasMaxLength(32);
        builder.HasIndex(x => x.Username).IsUnique();
    }
}
