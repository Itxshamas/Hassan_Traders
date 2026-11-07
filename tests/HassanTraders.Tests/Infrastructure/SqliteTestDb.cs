using HassanTraders.Desktop.Data.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace HassanTraders.Desktop.Data.Tests;

/// <summary>One in-memory SQLite database per test. The connection must stay open or the database vanishes.</summary>
internal sealed class SqliteTestDb : IDisposable
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:;Foreign Keys=True");

    public SqliteTestDb()
    {
        _connection.Open();
        Options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_connection).Options;
        using var context = CreateContext();
        context.Database.Migrate();
    }

    public DbContextOptions<AppDbContext> Options { get; }

    public AppDbContext CreateContext() => new(Options);

    public void Dispose() => _connection.Dispose();
}
