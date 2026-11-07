using HassanTraders.Desktop.Core.Common;
using HassanTraders.Desktop.Core.Security;
using HassanTraders.Desktop.Data.Persistence;
using HassanTraders.Desktop.Data.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HassanTraders.Desktop.Data.Tests;

public sealed class PersistenceTests : IDisposable
{
    private static readonly DateTime Now = new(2026, 5, 4, 10, 15, 0, DateTimeKind.Utc);
    private readonly SqliteTestDb _db = new();

    public void Dispose() => _db.Dispose();

    [Fact]
    public void Migrations_MatchTheModel()
    {
        using var context = _db.CreateContext();
        Assert.False(context.Database.HasPendingModelChanges(), "Add a migration: the model changed.");
    }

    [Fact]
    public async Task User_RoundTrips_WithUtcKindAndStringRole()
    {
        await using (var write = _db.CreateContext())
        {
            var user = User.Create("Admin", "Administrator", "hash", Role.Manager, true, Now);
            user.RecordLogin(Now.AddMinutes(5));
            await new UserRepository(write).AddAsync(user);
            await write.SaveChangesAsync();
        }

        await using var read = _db.CreateContext();
        var loaded = await new UserRepository(read).FindByUsernameAsync("admin");

        Assert.NotNull(loaded);
        Assert.Equal(Role.Manager, loaded.Role);
        Assert.Equal(Now, loaded.CreatedUtc);
        Assert.Equal(DateTimeKind.Utc, loaded.CreatedUtc.Kind);
        Assert.Equal(Now.AddMinutes(5), loaded.LastLoginUtc);
        Assert.Equal(DateTimeKind.Utc, loaded.LastLoginUtc!.Value.Kind);
        Assert.Equal("Manager", await read.Database.SqlQuery<string>($"SELECT Role AS Value FROM Users").SingleAsync());
    }

    [Fact]
    public async Task Username_MustBeUnique()
    {
        await using var context = _db.CreateContext();
        var repo = new UserRepository(context);
        await repo.AddAsync(User.Create("ali", "Ali", "h", Role.Cashier, false, Now));
        await repo.AddAsync(User.Create("ALI ", "Ali 2", "h", Role.Cashier, false, Now));

        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
    }

    [Fact]
    public async Task AuditEntries_ArePersistedWhenTheUnitOfWorkSaves()
    {
        await using (var write = _db.CreateContext())
        {
            var repo = new AuditRepository(write);
            repo.Add(new AuditEntry(Now, AuditAction.Login, 1, "admin", "Signed in"));
            repo.Add(new AuditEntry(Now, AuditAction.LoginFailed, null, "ghost", "Wrong username or password"));
            await write.SaveChangesAsync();
        }

        await using var read = _db.CreateContext();
        var rows = await read.AuditLog.OrderBy(a => a.Id).ToListAsync();
        Assert.Equal([AuditAction.Login, AuditAction.LoginFailed], rows.Select(r => r.Action));
        Assert.Null(rows[1].UserId);
        Assert.Equal(DateTimeKind.Utc, rows[0].TimestampUtc.Kind);
    }

    [Fact]
    public async Task Settings_SetIsAnUpsert()
    {
        await using (var first = _db.CreateContext())
        {
            await new SettingsRepository(first).SetAsync("ui.language", "ur");
            await first.SaveChangesAsync();
        }

        await using (var second = _db.CreateContext())
        {
            var repo = new SettingsRepository(second);
            Assert.Equal("ur", await repo.GetAsync("ui.language"));
            await repo.SetAsync("ui.language", "en");
            await second.SaveChangesAsync();
        }

        await using var third = _db.CreateContext();
        Assert.Equal("en", await new SettingsRepository(third).GetAsync("ui.language"));
        Assert.Null(await new SettingsRepository(third).GetAsync("missing"));
        Assert.Equal(1, await third.Settings.CountAsync());
    }

    [Fact]
    public void Money_IsNotMappedAsFloatingPoint()
    {
        // Rule 1: no double/decimal/float columns anywhere in the model.
        using var context = _db.CreateContext();
        var bad = context.Model.GetEntityTypes()
            .SelectMany(e => e.GetProperties())
            .Where(p => p.ClrType == typeof(double) || p.ClrType == typeof(decimal) || p.ClrType == typeof(float))
            .Select(p => $"{p.DeclaringType.DisplayName()}.{p.Name}")
            .ToList();
        Assert.Empty(bad);
    }
}
