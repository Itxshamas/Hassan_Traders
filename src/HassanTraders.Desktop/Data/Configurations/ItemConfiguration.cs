using HassanTraders.Desktop.Core.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HassanTraders.Desktop.Data.Persistence.Configurations;

internal sealed class ItemConfiguration : IEntityTypeConfiguration<Item>
{
    public void Configure(EntityTypeBuilder<Item> builder)
    {
        builder.ToTable("Items");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.UrduName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Brand).HasMaxLength(128).IsRequired();
        builder.Property(x => x.Barcode).HasMaxLength(64);
        builder.Property(x => x.Unit).HasConversion<string>().HasMaxLength(24);
        builder.Property(x => x.CostPrice).HasConversion(x => x.Paisa, x => new(x)).HasColumnType("INTEGER");
        builder.Property(x => x.SalePrice).HasConversion(x => x.Paisa, x => new(x)).HasColumnType("INTEGER");
        builder.Property(x => x.ReorderLevel).HasConversion(x => x.BaseUnits, x => new(x)).HasColumnType("INTEGER");
        builder.HasIndex(x => x.Barcode).IsUnique();
        builder.HasIndex(x => x.Name);
        builder.HasIndex(x => x.UrduName);
        builder.HasIndex(x => new { x.CategoryId, x.ProductTypeId });
        builder.HasOne(x => x.Category)
            .WithMany()
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.ProductType)
            .WithMany()
            .HasForeignKey(x => x.ProductTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
