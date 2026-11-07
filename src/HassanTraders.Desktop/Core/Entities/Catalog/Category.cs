using HassanTraders.Desktop.Core.Common;

namespace HassanTraders.Desktop.Core.Catalog;

public sealed class Category : Entity
{
    private Category()
    {
        Name = string.Empty;
        UrduName = string.Empty;
    }

    private Category(string name, string urduName, int displayOrder)
    {
        Name = Required(name, nameof(name));
        UrduName = Required(urduName, nameof(urduName));
        DisplayOrder = displayOrder;
        IsActive = true;
    }

    public string Name { get; private set; }

    public string UrduName { get; private set; }

    public int DisplayOrder { get; private set; }

    public bool IsActive { get; private set; }

    public static Category Create(string name, string urduName, int displayOrder = 0) =>
        new(name, urduName, displayOrder);

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
