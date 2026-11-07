using HassanTraders.Desktop.Services.Application;
using HassanTraders.Desktop.Services.Application.Abstractions;
using HassanTraders.Desktop.Services.Application.Catalog;
using HassanTraders.Desktop.Services.Application.Security;
using HassanTraders.Desktop.Core.Security;
using HassanTraders.Desktop.Data.Persistence;
using HassanTraders.Desktop.Data.Persistence.Seed;
using HassanTraders.Desktop.Data.Import;
using HassanTraders.Desktop.Data.Persistence.Repositories;
using HassanTraders.Desktop.Data.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

namespace HassanTraders.Desktop.Data.Tests;

public sealed class DatabaseInitializerTests : IDisposable
{
    private readonly SqliteTestDb _db = new();

    public void Dispose() => _db.Dispose();

    private static DatabaseInitializer CreateInitializer(AppDbContext context) =>
        new(
            context,
            new Pbkdf2PasswordHasher(1_000),
            new SystemClock(),
            new ItemCatalogImportService(new CatalogFileReader(), new ItemRepository(context), context),
            NullLogger<DatabaseInitializer>.Instance);

    [Fact]
    public async Task Initialize_SeedsOneAdmin_ThatMustChangeThePassword()
    {
        await using var context = _db.CreateContext();
        await CreateInitializer(context).InitializeAsync();

        var admin = await context.Users.SingleAsync();
        Assert.Equal("admin", admin.Username);
        Assert.Equal(Role.Owner, admin.Role);
        Assert.True(admin.MustChangePassword);
        Assert.NotEqual(AdminSeeder.DefaultPassword, admin.PasswordHash);
        Assert.True(new Pbkdf2PasswordHasher(1_000).Verify(AdminSeeder.DefaultPassword, admin.PasswordHash));
        Assert.Equal(38, await context.Items.CountAsync());
        Assert.Equal(4, await context.Variants.CountAsync());
    }

    [Fact]
    public async Task Initialize_IsIdempotent()
    {
        await using var context = _db.CreateContext();
        var initializer = CreateInitializer(context);
        await initializer.InitializeAsync();
        await initializer.InitializeAsync();

        Assert.Equal(1, await context.Users.CountAsync());
    }

    [Fact]
    public async Task EndToEnd_FirstLogin_ForcesPasswordChange_ThenNewPasswordWorks()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"ht-{Guid.NewGuid():N}", "test.db");
        try
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?> { [DependencyInjection.DatabasePathKey] = dbPath })
                .Build();
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddApplication();
            services.AddInfrastructure(config);
            await using var provider = services.BuildServiceProvider();
            var runner = provider.GetRequiredService<IUseCaseRunner>();

            await runner.RunAsync<IDatabaseInitializer>(i => i.InitializeAsync());
            Assert.True(File.Exists(dbPath), "The database is created on first run.");

            var login = await runner.RunAsync<IAuthService, HassanTraders.Desktop.Core.Common.Result<UserInfo>>(
                s => s.LoginAsync(new LoginRequest("Admin", AdminSeeder.DefaultPassword)));
            Assert.True(login.IsSuccess);
            Assert.True(login.Value.MustChangePassword);

            var change = await runner.RunAsync<IAuthService, HassanTraders.Desktop.Core.Common.Result>(
                s => s.ChangePasswordAsync(login.Value.Id, new ChangePasswordRequest(AdminSeeder.DefaultPassword, "Shop2026!x", "Shop2026!x")));
            Assert.True(change.IsSuccess);

            var oldPassword = await runner.RunAsync<IAuthService, HassanTraders.Desktop.Core.Common.Result<UserInfo>>(
                s => s.LoginAsync(new LoginRequest("admin", AdminSeeder.DefaultPassword)));
            Assert.Equal("Login_Invalid", oldPassword.Error);

            var again = await runner.RunAsync<IAuthService, HassanTraders.Desktop.Core.Common.Result<UserInfo>>(
                s => s.LoginAsync(new LoginRequest("admin", "Shop2026!x")));
            Assert.True(again.IsSuccess);
            Assert.False(again.Value.MustChangePassword);

            await using var scope = provider.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var actions = await db.AuditLog.OrderBy(a => a.Id).Select(a => a.Action).ToListAsync();
            Assert.Equal(
                [AuditAction.Login, AuditAction.PasswordChanged, AuditAction.LoginFailed, AuditAction.Login],
                actions);

            // The journal mode lives in the file, so a fresh connection must also see WAL.
            var conn = db.Database.GetDbConnection();
            await conn.OpenAsync();
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = "PRAGMA journal_mode;";
            Assert.Equal("wal", (string?)await cmd.ExecuteScalarAsync());
        }
        finally
        {
            Directory.Delete(Path.GetDirectoryName(dbPath)!, recursive: true);
        }
    }
}
