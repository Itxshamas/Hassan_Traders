using CommunityToolkit.Mvvm.ComponentModel;
using HassanTraders.Desktop.Core.DTOs.Catalog;
using HassanTraders.Desktop.Services;

namespace HassanTraders.Desktop.Features.Items;

/// <summary>
/// One row of the items grid. Holds both languages and shows the active language as the main line,
/// the other as a small second line (CLAUDE.md section 7).
/// </summary>
public sealed partial class ItemRowViewModel : ObservableObject
{
    private readonly CatalogItemRow _item;

    public ItemRowViewModel(CatalogItemRow item, CatalogFormatter formatter, ILocalizationService localization)
    {
        _item = item;
        SearchKey = SearchNormalizer.Normalize(string.Join(
            ' ',
            item.Name,
            item.UrduName,
            item.Brand,
            item.Barcode,
            item.CategoryName,
            item.CategoryUrduName,
            item.ProductTypeName,
            item.ProductTypeUrduName));
        SalePaisa = item.SalePrice.Paisa;
        CostPaisa = item.CostPrice.Paisa;
        PrimaryName = string.Empty;
        SecondaryName = string.Empty;
        CategoryLabel = string.Empty;
        PackLabel = string.Empty;
        CostText = string.Empty;
        PriceText = string.Empty;
        ReorderText = string.Empty;
        StatusText = string.Empty;
        ApplyLanguage(formatter, localization);
    }

    public long Id => _item.Id;

    public long CategoryId => _item.CategoryId;

    public long? ProductTypeId => _item.ProductTypeId;

    public string SearchKey { get; }

    public string Brand => _item.Brand;

    public string Barcode => _item.Barcode ?? string.Empty;

    public bool IsActive => _item.IsActive;

    /// <summary>Raw paisa values so the grid sorts numerically, not alphabetically.</summary>
    public long SalePaisa { get; }

    public long CostPaisa { get; }

    public int VariantCount => _item.VariantCount;

    [ObservableProperty]
    private string _primaryName;

    [ObservableProperty]
    private string _secondaryName;

    /// <summary>True when the second line is Urdu (UI is English), so the view can use the Nastaliq font for it.</summary>
    [ObservableProperty]
    private bool _secondaryIsUrdu;

    [ObservableProperty]
    private string _categoryLabel;

    [ObservableProperty]
    private string? _typeLabel;

    [ObservableProperty]
    private string _packLabel;

    [ObservableProperty]
    private string _costText;

    [ObservableProperty]
    private string _priceText;

    [ObservableProperty]
    private string _reorderText;

    [ObservableProperty]
    private string _statusText;

    public bool HasTypeLabel => !string.IsNullOrEmpty(TypeLabel);

    public bool HasVariants => _item.VariantCount > 0;

    [ObservableProperty]
    private string _variantsText = string.Empty;

    public void ApplyLanguage(CatalogFormatter formatter, ILocalizationService localization)
    {
        var urdu = localization.Language == "ur";
        PrimaryName = urdu ? _item.UrduName : _item.Name;
        SecondaryName = urdu ? _item.Name : _item.UrduName;
        SecondaryIsUrdu = !urdu;
        CategoryLabel = urdu ? _item.CategoryUrduName : _item.CategoryName;
        TypeLabel = urdu ? _item.ProductTypeUrduName : _item.ProductTypeName;
        OnPropertyChanged(nameof(HasTypeLabel));
        PackLabel = formatter.Pack(_item.Unit, _item.PackSize);
        CostText = _item.CostPrice.ToString();
        PriceText = _item.SalePrice.ToString();
        ReorderText = formatter.Amount(_item.Unit, _item.ReorderLevel);
        StatusText = localization[_item.IsActive ? "Status_Active" : "Status_Inactive"];
        VariantsText = HasVariants ? localization.Format("Items_Variants", _item.VariantCount) : string.Empty;
    }
}
