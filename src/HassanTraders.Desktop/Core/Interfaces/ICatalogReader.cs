using HassanTraders.Desktop.Core.DTOs.Catalog;

namespace HassanTraders.Desktop.Services.Application.Abstractions;

/// <summary>Read-only catalogue queries for lists and dashboards. Writes go through <see cref="IItemRepository"/>.</summary>
public interface ICatalogReader
{
    Task<CatalogListing> ReadAsync(CancellationToken cancellationToken = default);
}
