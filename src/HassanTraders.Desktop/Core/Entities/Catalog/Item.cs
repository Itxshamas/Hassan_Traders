using HassanTraders.Desktop.Core.Common;

namespace HassanTraders.Desktop.Core.Catalog;

public sealed class Item : Entity
{
    private Item()
    {
        Name = string.Empty;
        UrduName = string.Empty;
        Brand = string.Empty;
    }

    private Item(
        long categoryId,
        long? productTypeId,
        string name,
        string urduName,
        string? brand,
        string? barcode,
        UnitKind unit,
        long packSize,
        Money costPrice,
        Money salePrice,
        Quantity reorderLevel,
        int taxRateBasisPoints)
    {
        if (categoryId < 0 || productTypeId is < 0 || packSize < 0 || reorderLevel.BaseUnits < 0 ||
            !Enum.IsDefined(unit) || costPrice.IsNegative || salePrice.IsNegative)
        {
            throw new DomainException("Item category, unit, pack size, price, or reorder level is invalid.");
        }

        if (taxRateBasisPoints is < 0 or > 10_000)
        {
            throw new DomainException("Tax rate is invalid.");
        }

        CategoryId = categoryId;
        ProductTypeId = productTypeId;
        Name = Required(name, nameof(name));
        UrduName = Required(urduName, nameof(urduName));
        Brand = brand?.Trim() ?? string.Empty;
        Barcode = string.IsNullOrWhiteSpace(barcode) ? null : barcode.Trim();
        Unit = unit;
        PackSize = packSize;
        CostPrice = costPrice;
        SalePrice = salePrice;
        ReorderLevel = reorderLevel;
        TaxRateBasisPoints = taxRateBasisPoints;
        IsActive = true;
    }

    public long CategoryId { get; private set; }

    public Category? Category { get; private set; }

    public long? ProductTypeId { get; private set; }

    public ProductType? ProductType { get; private set; }

    public string Name { get; private set; }

    public string UrduName { get; private set; }

    public string Brand { get; private set; }

    public string? Barcode { get; private set; }

    public UnitKind Unit { get; private set; }

    public long PackSize { get; private set; }

    public Money CostPrice { get; private set; }

    public Money SalePrice { get; private set; }

    public Quantity ReorderLevel { get; private set; }

    public int TaxRateBasisPoints { get; private set; }

    public bool IsActive { get; private set; }

    public static Item Create(
        long categoryId,
        long? productTypeId,
        string name,
        string urduName,
        string? brand,
        string? barcode,
        UnitKind unit,
        long packSize,
        Money costPrice,
        Money salePrice,
        Quantity reorderLevel,
        int taxRateBasisPoints = 0)
    {
        if (categoryId <= 0 || productTypeId is <= 0)
        {
            throw new DomainException("A saved category and product type are required.");
        }

        return new Item(categoryId, productTypeId, name, urduName, brand, barcode, unit, packSize, costPrice, salePrice, reorderLevel, taxRateBasisPoints);
    }

    public static Item Create(
        Category category,
        ProductType? productType,
        string name,
        string urduName,
        string? brand,
        string? barcode,
        UnitKind unit,
        long packSize,
        Money costPrice,
        Money salePrice,
        Quantity reorderLevel,
        int taxRateBasisPoints = 0)
    {
        ArgumentNullException.ThrowIfNull(category);
        if (productType is not null &&
            !ReferenceEquals(productType.Category, category) &&
            (category.Id == 0 || productType.CategoryId != category.Id))
        {
            throw new DomainException("An item type must belong to its item category.");
        }

        var item = new Item(
            category.Id,
            productType?.Id,
            name,
            urduName,
            brand,
            barcode,
            unit,
            packSize,
            costPrice,
            salePrice,
            reorderLevel,
            taxRateBasisPoints)
        {
            Category = category,
            ProductType = productType,
        };
        return item;
    }

    public void ChangePrices(Money costPrice, Money salePrice)
    {
        if (costPrice.IsNegative || salePrice.IsNegative)
        {
            throw new DomainException("Item prices cannot be negative.");
        }

        CostPrice = costPrice;
        SalePrice = salePrice;
    }

    public void SetActive(bool isActive) => IsActive = isActive;

    private static string Required(string value, string parameter) =>
        string.IsNullOrWhiteSpace(value)
            ? throw new DomainException($"{parameter} is required.")
            : value.Trim();
}
