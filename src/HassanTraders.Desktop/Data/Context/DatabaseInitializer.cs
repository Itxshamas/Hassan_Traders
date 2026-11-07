using System.IO;
using HassanTraders.Desktop.Services.Application.Abstractions;
using HassanTraders.Desktop.Services.Application.Catalog;
using HassanTraders.Desktop.Data.Persistence.Repositories;
using HassanTraders.Desktop.Data.Persistence.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HassanTraders.Desktop.Data.Persistence;

internal sealed class DatabaseInitializer : IDatabaseInitializer
{
    private readonly AppDbContext _db;
    private readonly IPasswordHasher _hasher;
    private readonly IClock _clock;
    private readonly ILogger<DatabaseInitializer> _logger;
    private readonly IItemCatalogImportService _catalogImport;

    public DatabaseInitializer(
        AppDbContext db,
        IPasswordHasher hasher,
        IClock clock,
        IItemCatalogImportService catalogImport,
        ILogger<DatabaseInitializer> logger)
    {
        _db = db;
        _hasher = hasher;
        _clock = clock;
        _catalogImport = catalogImport;
        _logger = logger;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        var connectionString = _db.Database.GetConnectionString();
        var directory = new Microsoft.Data.Sqlite.SqliteConnectionStringBuilder(connectionString).DataSource;
        if (!string.IsNullOrEmpty(directory) && directory != ":memory:" && Path.GetDirectoryName(Path.GetFullPath(directory)) is { } dir)
        {
            Directory.CreateDirectory(dir);
        }

        await _db.Database.MigrateAsync(cancellationToken);
        await EnableWalAsync(cancellationToken);

        var seeder = new AdminSeeder(new UserRepository(_db), _hasher, _clock);
        if (await seeder.SeedAsync(cancellationToken))
        {
            await _db.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Seeded default admin user.");
        }

        await SeedCatalogAsync(cancellationToken);
    }

    private async Task SeedCatalogAsync(CancellationToken cancellationToken)
    {
        if (await _db.Items.AnyAsync(cancellationToken))
        {
            return;
        }

        var resourceName = typeof(DatabaseInitializer).Assembly.GetManifestResourceNames()
            .SingleOrDefault(name => name.EndsWith(".Data.Seed.items.csv", StringComparison.Ordinal));
        if (resourceName is null)
        {
            throw new InvalidOperationException("The embedded sample item catalogue was not found.");
        }

        await using var stream = typeof(DatabaseInitializer).Assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException("The embedded sample item catalogue could not be opened.");
        var report = await _catalogImport.ImportAsync(stream, "items.csv", cancellationToken);
        if (report.Errors.Count > 0)
        {
            var details = string.Join("; ", report.Errors.Select(error => $"row {error.RowNumber}: {error.Message}"));
            throw new InvalidDataException($"The embedded sample catalogue contains invalid rows: {details}");
        }

        _logger.LogInformation("Seeded {ItemCount} sample catalogue items.", report.ImportedCount);
    }

    private async Task EnableWalAsync(CancellationToken cancellationToken)
    {
        // The journal mode is stored in the database file, so this is only needed once, but it is cheap and idempotent.
        // In-memory databases report "memory" and ignore the request.
        var connection = _db.Database.GetDbConnection();
        var wasClosed = connection.State != System.Data.ConnectionState.Open;
        if (wasClosed)
        {
            await connection.OpenAsync(cancellationToken);
        }

        try
        {
            await using var command = connection.CreateCommand();
            command.CommandText = "PRAGMA journal_mode=WAL;";
            var mode = await command.ExecuteScalarAsync(cancellationToken);
            _logger.LogInformation("SQLite journal mode: {Mode}", mode);
        }
        finally
        {
            if (wasClosed)
            {
                await connection.CloseAsync();
            }
        }
    }
}
