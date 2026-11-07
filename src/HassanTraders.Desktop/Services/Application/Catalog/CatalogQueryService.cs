using HassanTraders.Desktop.Core.DTOs.Catalog;
using HassanTraders.Desktop.Services.Application.Abstractions;

namespace HassanTraders.Desktop.Services.Application.Catalog;

public interface ICatalogQueryService
{
    Task<CatalogListing> GetListingAsync(CancellationToken cancellationToken = default);

    Task<CatalogSummary> GetSummaryAsync(CancellationToken cancellationToken = default);
}

public sealed class CatalogQueryService : ICatalogQueryService
{
    private readonly ICatalogReader _reader;

    public CatalogQueryService(ICatalogReader reader) => _reader = reader;

    public Task<CatalogListing> GetListingAsync(CancellationToken cancellationToken = default) =>
        _reader.ReadAsync(cancellationToken);

    public async Task<CatalogSummary> GetSummaryAsync(CancellationToken cancellationToken = default)
    {
        var listing = await _reader.ReadAsync(cancellationToken);
        return Summarize(listing);
    }

    internal static CatalogSummary Summarize(CatalogListing listing)
    {
        var active = listing.Items.Where(i => i.IsActive).ToList();
        var breakdown = listing.Categories
            .Select(c => new CategoryBreakdown(
                c.Name,
                c.UrduName,
                active.Count(i => i.CategoryId == c.Id),
                c.Types
                    .Select(t => new TypeBreakdown(t.Name, t.UrduName, active.Count(i => i.ProductTypeId == t.Id)))
                    .ToList()))
            .ToList();

        return new CatalogSummary(
            active.Count,
            listing.Items.Count - active.Count,
            listing.Categories.Count,
            listing.Categories.Sum(c => c.Types.Count),
            listing.Items.Sum(i => i.VariantCount),
            breakdown);
    }
}
