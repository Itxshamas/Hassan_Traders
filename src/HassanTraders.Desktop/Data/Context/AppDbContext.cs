using HassanTraders.Desktop.Services.Application.Abstractions;
using HassanTraders.Desktop.Core.Catalog;
using HassanTraders.Desktop.Core.Common;
using HassanTraders.Desktop.Core.Security;
using Microsoft.EntityFrameworkCore;

namespace HassanTraders.Desktop.Data.Persistence;

public sealed class AppDbContext : DbContext, IUnitOfWork
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();

    public DbSet<AuditEntry> AuditLog => Set<AuditEntry>();

    public DbSet<Setting> Settings => Set<Setting>();

    public DbSet<Category> Categories => Set<Category>();

    public DbSet<ProductType> ProductTypes => Set<ProductType>();

    public DbSet<Item> Items => Set<Item>();

    public DbSet<Variant> Variants => Set<Variant>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        // SQLite has no date type. Always store and read UTC so values round-trip with Kind = Utc.
        configurationBuilder.Properties<DateTime>().HaveConversion<UtcDateTimeConverter>();
        configurationBuilder.Properties<DateTime?>().HaveConversion<NullableUtcDateTimeConverter>();
    }
}
