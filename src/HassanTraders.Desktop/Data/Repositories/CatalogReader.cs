using HassanTraders.Desktop.Core.DTOs.Catalog;
using HassanTraders.Desktop.Services.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace HassanTraders.Desktop.Data.Persistence.Repositories;

internal sealed class CatalogReader : ICatalogReader
{
    private readonly AppDbContext _db;

    public CatalogReader(AppDbContext db) => _db = db;

    public async Task<CatalogListing> ReadAsync(CancellationToken cancellationToken = default)
    {
        var categories = await _db.Categories
            .AsNoTracking()
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Name)
            .Select(c => new { c.Id, c.Name, c.UrduName })
            .ToListAsync(cancellationToken);

        var types = await _db.ProductTypes
            .AsNoTracking()
            .OrderBy(t => t.DisplayOrder)
            .ThenBy(t => t.Name)
            .Select(t => new { t.Id, t.CategoryId, t.Name, t.UrduName })
            .ToListAsync(cancellationToken);

        var items = await _db.Items
            .AsNoTracking()
            .OrderBy(i => i.Name)
            .Select(i => new CatalogItemRow(
                i.Id,
                i.Name,
                i.UrduName,
                i.Brand,
                i.Barcode,
                i.CategoryId,
                i.Category!.Name,
                i.Category!.UrduName,
                i.ProductTypeId,
                i.ProductType == null ? null : i.ProductType.Name,
                i.ProductType == null ? null : i.ProductType.UrduName,
                i.Unit,
                i.PackSize,
                i.CostPrice,
                i.SalePrice,
                i.ReorderLevel,
                i.IsActive,
                _db.Variants.Count(v => v.ItemId == i.Id)))
            .ToListAsync(cancellationToken);

        var categoryRows = categories
            .Select(c => new CatalogCategoryRow(
                c.Id,
                c.Name,
                c.UrduName,
                types.Where(t => t.CategoryId == c.Id)
                    .Select(t => new CatalogTypeRow(t.Id, t.Name, t.UrduName))
                    .ToList()))
            .ToList();

        return new CatalogListing(items, categoryRows);
    }
}
