using System.IO;
using HassanTraders.Desktop.Services.Application.Abstractions;
using HassanTraders.Desktop.Core.Catalog;
using HassanTraders.Desktop.Core.Common;

namespace HassanTraders.Desktop.Services.Application.Catalog;

public sealed class ItemCatalogImportService : IItemCatalogImportService
{
    private readonly ICatalogFileReader _reader;
    private readonly IItemRepository _items;
    private readonly IUnitOfWork _unitOfWork;

    public ItemCatalogImportService(ICatalogFileReader reader, IItemRepository items, IUnitOfWork unitOfWork)
    {
        _reader = reader;
        _items = items;
        _unitOfWork = unitOfWork;
    }

    public async Task<ItemImportReport> ImportAsync(Stream file, string fileName, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(file);
        var rows = await _reader.ReadAsync(file, fileName, cancellationToken);
        var errors = new List<ImportRowError>();
        var imported = 0;
        var seenBarcodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var row in rows)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!TryParse(row, out var unit, out var packSize, out var cost, out var sale, out var reorder, out var error))
            {
                errors.Add(new ImportRowError(row.RowNumber, error));
                continue;
            }

            var barcode = row.Barcode.Trim();
            if (barcode.Length > 0 && (seenBarcodes.Contains(barcode) || await _items.BarcodeExistsAsync(barcode, cancellationToken)))
            {
                errors.Add(new ImportRowError(row.RowNumber, "Duplicate barcode."));
                continue;
            }

            var category = await _items.FindCategoryAsync(row.Category.Trim(), cancellationToken);
            if (category is null)
            {
                category = Category.Create(row.Category, row.CategoryUrdu, imported);
                _items.Add(category);
            }
            else if (!string.Equals(category.UrduName, row.CategoryUrdu.Trim(), StringComparison.Ordinal))
            {
                errors.Add(new ImportRowError(row.RowNumber, "Category Urdu name does not match the existing category."));
                continue;
            }

            ProductType? productType = null;
            if (!string.IsNullOrWhiteSpace(row.ProductType))
            {
                productType = await _items.FindProductTypeAsync(category, row.ProductType.Trim(), cancellationToken);
                if (productType is null)
                {
                    productType = ProductType.Create(category, row.ProductType, row.ProductTypeUrdu, imported);
                    _items.Add(productType);
                }
                else if (!string.Equals(productType.UrduName, row.ProductTypeUrdu.Trim(), StringComparison.Ordinal))
                {
                    errors.Add(new ImportRowError(row.RowNumber, "Product type Urdu name does not match the existing type."));
                    continue;
                }
            }

            var item = await _items.FindItemAsync(category, productType, row.Name.Trim(), cancellationToken);
            if (item is not null && string.IsNullOrWhiteSpace(row.VariantName))
            {
                errors.Add(new ImportRowError(row.RowNumber, "Duplicate item name within the category."));
                continue;
            }

            if (!TryParseVariant(row, out var variantPackSize, out var variantCost, out var variantSale, out error))
            {
                errors.Add(new ImportRowError(row.RowNumber, error));
                continue;
            }

            if (item is null)
            {
                item = Item.Create(
                    category,
                    productType,
                    row.Name,
                    row.NameUrdu,
                    row.Brand,
                    barcode,
                    unit,
                    packSize,
                    cost,
                    sale,
                    reorder);
                _items.Add(item);
            }

            if (!string.IsNullOrWhiteSpace(row.VariantName))
            {
                if (await _items.FindVariantAsync(item, row.VariantName.Trim(), cancellationToken) is not null)
                {
                    errors.Add(new ImportRowError(row.RowNumber, "Duplicate variant name."));
                    continue;
                }

                _items.Add(Variant.Create(item, row.VariantName, variantPackSize, variantCost, variantSale));
            }

            if (barcode.Length > 0)
            {
                seenBarcodes.Add(barcode);
            }

            imported++;
        }

        if (imported > 0)
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return new ItemImportReport(imported, errors);
    }

    private static bool TryParse(
        ItemImportRow row,
        out UnitKind unit,
        out long packSize,
        out Money cost,
        out Money sale,
        out Quantity reorder,
        out string error)
    {
        unit = default;
        packSize = 0;
        cost = Money.Zero;
        sale = Money.Zero;
        reorder = Quantity.Zero;

        if (string.IsNullOrWhiteSpace(row.Category) ||
            string.IsNullOrWhiteSpace(row.CategoryUrdu) ||
            string.IsNullOrWhiteSpace(row.Name) ||
            string.IsNullOrWhiteSpace(row.NameUrdu) ||
            row.Category.Length > 128 ||
            row.CategoryUrdu.Length > 128 ||
            row.ProductType.Length > 128 ||
            row.ProductTypeUrdu.Length > 128 ||
            row.Name.Length > 200 ||
            row.NameUrdu.Length > 200 ||
            row.Brand.Length > 128 ||
            row.Barcode.Length > 64 ||
            row.VariantName.Length > 128)
        {
            error = "A required name is missing or a field exceeds its maximum length.";
            return false;
        }

        if (!string.IsNullOrWhiteSpace(row.ProductType) != !string.IsNullOrWhiteSpace(row.ProductTypeUrdu))
        {
            error = "Product type names must be provided in both languages.";
            return false;
        }

        if (!Enum.TryParse(row.Unit, true, out unit) || !Enum.IsDefined(unit) ||
            !long.TryParse(row.PackSizeBaseUnits, out packSize) || packSize < 0 ||
            !long.TryParse(row.CostPaisa, out var costPaisa) || costPaisa < 0 ||
            !long.TryParse(row.SalePaisa, out var salePaisa) || salePaisa < 0 ||
            !long.TryParse(row.ReorderBaseUnits, out var reorderBaseUnits) || reorderBaseUnits < 0)
        {
            error = "Unit, pack size, prices, or reorder quantity is invalid.";
            return false;
        }

        cost = new Money(costPaisa);
        sale = new Money(salePaisa);
        reorder = new Quantity(reorderBaseUnits);
        error = string.Empty;
        return true;
    }

    private static bool TryParseVariant(
        ItemImportRow row,
        out long packSize,
        out Money cost,
        out Money sale,
        out string error)
    {
        packSize = 0;
        cost = Money.Zero;
        sale = Money.Zero;
        var hasVariant = !string.IsNullOrWhiteSpace(row.VariantName);
        var hasVariantValues = !string.IsNullOrWhiteSpace(row.VariantPackSizeBaseUnits) ||
            !string.IsNullOrWhiteSpace(row.VariantCostPaisa) ||
            !string.IsNullOrWhiteSpace(row.VariantSalePaisa);
        if (!hasVariant && !hasVariantValues)
        {
            error = string.Empty;
            return true;
        }

        if (!hasVariant ||
            !long.TryParse(row.VariantPackSizeBaseUnits, out packSize) || packSize <= 0 ||
            !long.TryParse(row.VariantCostPaisa, out var costPaisa) || costPaisa < 0 ||
            !long.TryParse(row.VariantSalePaisa, out var salePaisa) || salePaisa < 0)
        {
            error = "Variant name, pack size, or prices are invalid.";
            return false;
        }

        cost = new Money(costPaisa);
        sale = new Money(salePaisa);
        error = string.Empty;
        return true;
    }
}
