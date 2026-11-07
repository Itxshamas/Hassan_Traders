using HassanTraders.Desktop.Core.Common;

namespace HassanTraders.Desktop.Core.Catalog;

public sealed class Variant : Entity
{
    private Variant() => Name = string.Empty;

    private Variant(long itemId, string name, long packSize, Money costPrice, Money salePrice)
    {
        if (itemId < 0 || packSize <= 0 || costPrice.IsNegative || salePrice.IsNegative)
        {
            throw new DomainException("Variant item, pack size, or prices are invalid.");
        }

        ItemId = itemId;
        Name = string.IsNullOrWhiteSpace(name)
            ? throw new DomainException("Variant name is required.")
            : name.Trim();
        PackSize = packSize;
        CostPrice = costPrice;
        SalePrice = salePrice;
        IsActive = true;
    }

    public long ItemId { get; private set; }

    public Item? Item { get; private set; }

    public string Name { get; private set; }

    public long PackSize { get; private set; }

    public Money CostPrice { get; private set; }

    public Money SalePrice { get; private set; }

    public bool IsActive { get; private set; }

    public static Variant Create(long itemId, string name, long packSize, Money costPrice, Money salePrice)
    {
        if (itemId <= 0)
        {
            throw new DomainException("A variant must belong to a saved item.");
        }

        return new Variant(itemId, name, packSize, costPrice, salePrice);
    }

    public static Variant Create(Item item, string name, long packSize, Money costPrice, Money salePrice)
    {
        ArgumentNullException.ThrowIfNull(item);
        var variant = new Variant(item.Id, name, packSize, costPrice, salePrice)
        {
            Item = item,
        };
        return variant;
    }

    public void SetActive(bool isActive) => IsActive = isActive;
}
