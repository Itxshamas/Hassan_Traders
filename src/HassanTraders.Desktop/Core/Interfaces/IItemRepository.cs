using HassanTraders.Desktop.Core.Catalog;

namespace HassanTraders.Desktop.Services.Application.Abstractions;

public interface IItemRepository
{
    Task<Category?> FindCategoryAsync(string name, CancellationToken cancellationToken = default);

    Task<ProductType?> FindProductTypeAsync(Category category, string name, CancellationToken cancellationToken = default);

    Task<Item?> FindItemAsync(Category category, ProductType? productType, string name, CancellationToken cancellationToken = default);

    Task<Variant?> FindVariantAsync(Item item, string name, CancellationToken cancellationToken = default);

    Task<bool> BarcodeExistsAsync(string barcode, CancellationToken cancellationToken = default);

    void Add(Category category);

    void Add(ProductType productType);

    void Add(Item item);

    void Add(Variant variant);
}
