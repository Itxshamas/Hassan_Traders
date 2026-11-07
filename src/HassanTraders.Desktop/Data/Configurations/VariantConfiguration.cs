using HassanTraders.Desktop.Core.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HassanTraders.Desktop.Data.Persistence.Configurations;

internal sealed class VariantConfiguration : IEntityTypeConfiguration<Variant>
{
    public void Configure(EntityTypeBuilder<Variant> builder)
    {
        builder.ToTable("Variants");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(128).IsRequired();
        builder.Property(x => x.CostPrice).HasConversion(x => x.Paisa, x => new(x)).HasColumnType("INTEGER");
        builder.Property(x => x.SalePrice).HasConversion(x => x.Paisa, x => new(x)).HasColumnType("INTEGER");
        builder.HasIndex(x => new { x.ItemId, x.Name }).IsUnique();
        builder.HasOne(x => x.Item)
            .WithMany()
            .HasForeignKey(x => x.ItemId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
