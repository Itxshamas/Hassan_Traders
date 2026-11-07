using HassanTraders.Desktop.Services.Application.Abstractions;
using HassanTraders.Desktop.Core.Catalog;
using Microsoft.EntityFrameworkCore;

namespace HassanTraders.Desktop.Data.Persistence.Repositories;

internal sealed class ItemRepository : IItemRepository
{
    private readonly AppDbContext _db;

    public ItemRepository(AppDbContext db) => _db = db;

    public async Task<Category?> FindCategoryAsync(string name, CancellationToken cancellationToken = default)
    {
        var local = _db.Categories.Local.FirstOrDefault(x =>
            string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));
        return local ?? await _db.Categories.SingleOrDefaultAsync(
            x => EF.Functions.Collate(x.Name, "NOCASE") == name,
            cancellationToken);
    }

    public async Task<ProductType?> FindProductTypeAsync(
        Category category,
        string name,
        CancellationToken cancellationToken = default)
    {
        var local = _db.ProductTypes.Local.FirstOrDefault(x =>
            (ReferenceEquals(x.Category, category) || (category.Id != 0 && x.CategoryId == category.Id)) &&
            string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));
        if (local is not null || category.Id == 0)
        {
            return local;
        }

        return await _db.ProductTypes.SingleOrDefaultAsync(
            x => x.CategoryId == category.Id && EF.Functions.Collate(x.Name, "NOCASE") == name,
            cancellationToken);
    }

    public async Task<Item?> FindItemAsync(
        Category category,
        ProductType? productType,
        string name,
        CancellationToken cancellationToken = default)
    {
        var local = _db.Items.Local.FirstOrDefault(x =>
            (ReferenceEquals(x.Category, category) || (category.Id != 0 && x.CategoryId == category.Id)) &&
            (ReferenceEquals(x.ProductType, productType) ||
             (productType is not null && productType.Id != 0 && x.ProductTypeId == productType.Id) ||
             (productType is null && x.ProductTypeId is null)) &&
            string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));
        if (local is not null || category.Id == 0)
        {
            return local;
        }

        var query = _db.Items.Where(x => x.CategoryId == category.Id);
        query = productType is null
            ? query.Where(x => x.ProductTypeId == null)
            : query.Where(x => x.ProductTypeId == productType.Id);
        return await query.SingleOrDefaultAsync(
            x => EF.Functions.Collate(x.Name, "NOCASE") == name,
            cancellationToken);
    }

    public async Task<Variant?> FindVariantAsync(Item item, string name, CancellationToken cancellationToken = default)
    {
        var local = _db.Variants.Local.FirstOrDefault(x =>
            (ReferenceEquals(x.Item, item) || (item.Id != 0 && x.ItemId == item.Id)) &&
            string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));
        if (local is not null || item.Id == 0)
        {
            return local;
        }

        return await _db.Variants.SingleOrDefaultAsync(
            x => x.ItemId == item.Id && EF.Functions.Collate(x.Name, "NOCASE") == name,
            cancellationToken);
    }

    public async Task<bool> BarcodeExistsAsync(string barcode, CancellationToken cancellationToken = default)
    {
        if (_db.Items.Local.Any(x => string.Equals(x.Barcode, barcode, StringComparison.OrdinalIgnoreCase)))
        {
            return true;
        }

        return await _db.Items.AnyAsync(
            x => x.Barcode != null && EF.Functions.Collate(x.Barcode, "NOCASE") == barcode,
            cancellationToken);
    }

    public void Add(Category category) => _db.Categories.Add(category);

    public void Add(ProductType productType) => _db.ProductTypes.Add(productType);

    public void Add(Item item) => _db.Items.Add(item);

    public void Add(Variant variant) => _db.Variants.Add(variant);
}
