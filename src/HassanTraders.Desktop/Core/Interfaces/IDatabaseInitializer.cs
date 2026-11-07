namespace HassanTraders.Desktop.Services.Application.Abstractions;

public interface IDatabaseInitializer
{
    /// <summary>Creates the database on first run, applies migrations, switches on WAL and seeds required data.</summary>
    Task InitializeAsync(CancellationToken cancellationToken = default);
}
