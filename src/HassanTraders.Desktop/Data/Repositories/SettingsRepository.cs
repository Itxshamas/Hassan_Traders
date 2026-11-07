using HassanTraders.Desktop.Services.Application.Abstractions;
using HassanTraders.Desktop.Core.Common;
using Microsoft.EntityFrameworkCore;

namespace HassanTraders.Desktop.Data.Persistence.Repositories;

internal sealed class SettingsRepository : ISettingsRepository
{
    private readonly AppDbContext _db;

    public SettingsRepository(AppDbContext db) => _db = db;

    public async Task<string?> GetAsync(string key, CancellationToken cancellationToken = default)
    {
        var setting = await _db.Settings.AsNoTracking().FirstOrDefaultAsync(s => s.Key == key, cancellationToken);
        return setting?.Value;
    }

    public async Task SetAsync(string key, string value, CancellationToken cancellationToken = default)
    {
        var existing = await _db.Settings.FirstOrDefaultAsync(s => s.Key == key, cancellationToken);
        if (existing is null)
        {
            _db.Settings.Add(new Setting(key, value));
        }
        else
        {
            existing.Change(value);
        }
    }
}
