using HassanTraders.Desktop.Services.Application.Abstractions;
using HassanTraders.Desktop.Services.Application.Catalog;
using HassanTraders.Desktop.Data.Import;
using HassanTraders.Desktop.Data.Persistence;
using HassanTraders.Desktop.Data.Persistence.Repositories;
using HassanTraders.Desktop.Data.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HassanTraders.Desktop.Data;

public static class DependencyInjection
{
    /// <summary>Config key that overrides the database file location (used for tests and backups).</summary>
    public const string DatabasePathKey = "Database:Path";

    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var path = configuration[DatabasePathKey];
        if (string.IsNullOrWhiteSpace(path))
        {
            path = AppPaths.DefaultDatabasePath;
        }

        // Pooling off: the file must be free to copy or replace (backup and restore) once connections close.
        var connectionString = $"Data Source={path};Foreign Keys=True;Pooling=False";

        services.AddDbContext<AppDbContext>(o => o.UseSqlite(connectionString));
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IAuditRepository, AuditRepository>();
        services.AddScoped<ISettingsRepository, SettingsRepository>();
        services.AddScoped<IItemRepository, ItemRepository>();
        services.AddScoped<ICatalogReader, CatalogReader>();
        services.AddScoped<ICatalogFileReader, CatalogFileReader>();
        services.AddScoped<IDatabaseInitializer, DatabaseInitializer>();
        services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();
        services.AddSingleton<IClock, SystemClock>();
        return services;
    }
}
