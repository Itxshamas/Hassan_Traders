using HassanTraders.Desktop.Core.Common;

namespace HassanTraders.Desktop.Core.Catalog;

public sealed class ProductType : Entity
{
    private ProductType()
    {
        Name = string.Empty;
        UrduName = string.Empty;
    }

    private ProductType(long categoryId, string name, string urduName, int displayOrder)
    {
        if (categoryId < 0)
        {
            throw new DomainException("A product type must belong to a category.");
        }

        CategoryId = categoryId;
        Name = Required(name, nameof(name));
        UrduName = Required(urduName, nameof(urduName));
        DisplayOrder = displayOrder;
        IsActive = true;
    }

    public long CategoryId { get; private set; }

    public Category? Category { get; private set; }

    public string Name { get; private set; }

    public string UrduName { get; private set; }

    public int DisplayOrder { get; private set; }

    public bool IsActive { get; private set; }

    public static ProductType Create(long categoryId, string name, string urduName, int displayOrder = 0)
    {
        if (categoryId <= 0)
        {
            throw new DomainException("A product type must belong to a saved category.");
        }

        return new ProductType(categoryId, name, urduName, displayOrder);
    }

    public static ProductType Create(Category category, string name, string urduName, int displayOrder = 0)
    {
        ArgumentNullException.ThrowIfNull(category);
        var productType = new ProductType(category.Id, name, urduName, displayOrder)
        {
            Category = category,
        };
        return productType;
    }

    public void Rename(string name, string urduName)
    {
        Name = Required(name, nameof(name));
        UrduName = Required(urduName, nameof(urduName));
    }

    public void SetActive(bool isActive) => IsActive = isActive;

    private static string Required(string value, string parameter) =>
        string.IsNullOrWhiteSpace(value)
            ? throw new DomainException($"{parameter} is required.")
            : value.Trim();
}
