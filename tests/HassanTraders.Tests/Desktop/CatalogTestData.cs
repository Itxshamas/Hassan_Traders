using HassanTraders.Desktop.Core.Catalog;
using HassanTraders.Desktop.Core.Common;
using HassanTraders.Desktop.Core.DTOs.Catalog;

namespace HassanTraders.Desktop.Tests;

internal static class CatalogTestData
{
    public static CatalogItemRow Lays { get; } = new(
        1, "Lays Classic", "لیز کلاسک", "Lays", null, 1, "Snacks", "نمکو اور اسنیکس", null, null, null,
        UnitKind.Piece, 1, new Money(1_800), new Money(2_000), new Quantity(12), true, 4);

    public static CatalogItemRow RiceSupreme { get; } = new(
        2, "Inam Supreme Rice", "انعام سپریم چاول", "Inam", "8901", 2, "Rice", "چاول", 10, "Premium", "پریمیم",
        UnitKind.Bag, 25_000, new Money(580_000), new Money(620_000), new Quantity(50_000), true, 0);

    public static CatalogItemRow RiceIrri { get; } = new(
        3, "Irri-9 Rice", "اری ۹ چاول", string.Empty, null, 2, "Rice", "چاول", 11, "Economical", "اقتصادی",
        UnitKind.Bag, 25_000, new Money(340_000), new Money(380_000), new Quantity(50_000), true, 0);

    public static CatalogItemRow LooseSugar { get; } = new(
        4, "Sugar Loose", "چینی کھلی", string.Empty, null, 3, "Dal and sugar", "دال اور چینی", null, null, null,
        UnitKind.Kilogram, 0, new Money(15_000), new Money(17_000), new Quantity(25_000), false, 0);

    public static CatalogListing Listing { get; } = new(
        [Lays, RiceSupreme, RiceIrri, LooseSugar],
        [
            new CatalogCategoryRow(1, "Snacks", "نمکو اور اسنیکس", []),
            new CatalogCategoryRow(2, "Rice", "چاول", [new CatalogTypeRow(10, "Premium", "پریمیم"), new CatalogTypeRow(11, "Economical", "اقتصادی")]),
            new CatalogCategoryRow(3, "Dal and sugar", "دال اور چینی", []),
        ]);
}
