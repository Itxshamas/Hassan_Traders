namespace HassanTraders.Desktop.Services.Application.Abstractions;

public interface ISettingsRepository
{
    Task<string?> GetAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>Stages an upsert; it is written when the unit of work saves.</summary>
    Task SetAsync(string key, string value, CancellationToken cancellationToken = default);
}
