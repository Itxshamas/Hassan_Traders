using HassanTraders.Desktop.Core.Catalog;
using HassanTraders.Desktop.Core.Common;

namespace HassanTraders.Desktop.Core.DTOs.Catalog;

/// <summary>One catalogue item as shown in lists: names in both languages, category and type, prices, pack, status.</summary>
public sealed record CatalogItemRow(
    long Id,
    string Name,
    string UrduName,
    string Brand,
    string? Barcode,
    long CategoryId,
    string CategoryName,
    string CategoryUrduName,
    long? ProductTypeId,
    string? ProductTypeName,
    string? ProductTypeUrduName,
    UnitKind Unit,
    long PackSize,
    Money CostPrice,
    Money SalePrice,
    Quantity ReorderLevel,
    bool IsActive,
    int VariantCount);

public sealed record CatalogTypeRow(long Id, string Name, string UrduName);

public sealed record CatalogCategoryRow(long Id, string Name, string UrduName, IReadOnlyList<CatalogTypeRow> Types);

public sealed record CatalogListing(IReadOnlyList<CatalogItemRow> Items, IReadOnlyList<CatalogCategoryRow> Categories);

public sealed record TypeBreakdown(string Name, string UrduName, int ItemCount);

public sealed record CategoryBreakdown(string Name, string UrduName, int ItemCount, IReadOnlyList<TypeBreakdown> Types);

/// <summary>Real counts taken from the catalogue. Nothing here is estimated.</summary>
public sealed record CatalogSummary(
    int ActiveItems,
    int InactiveItems,
    int CategoryCount,
    int TypeCount,
    int VariantCount,
    IReadOnlyList<CategoryBreakdown> Categories);
