using HassanTraders.Desktop.Core.Catalog;
using HassanTraders.Desktop.Core.Common;
using HassanTraders.Desktop.Services;

namespace HassanTraders.Desktop.Tests;

public class CatalogFormatterTests
{
    private readonly LocalizationService _loc = TestSupport.NewLocalization();

    [Theory]
    [InlineData(UnitKind.Bag, 25_000, "25 kg bag")]
    [InlineData(UnitKind.Bag, 50_000, "50 kg bag")]
    [InlineData(UnitKind.Bag, 0, "Bag")]
    [InlineData(UnitKind.Kilogram, 0, "Loose, per kg")]
    [InlineData(UnitKind.Kilogram, 1_000, "1 kg")]
    [InlineData(UnitKind.Gram, 430, "430 g")]
    [InlineData(UnitKind.Gram, 1_000, "1 kg")]
    [InlineData(UnitKind.Millilitre, 500, "500 mL")]
    [InlineData(UnitKind.Litre, 1_000, "1 L")]
    [InlineData(UnitKind.Piece, 1, "Piece")]
    [InlineData(UnitKind.Dozen, 12, "Dozen")]
    [InlineData(UnitKind.Carton, 24, "Carton")]
    public void Pack_ReadsNaturallyInEnglish(UnitKind unit, long packSize, string expected)
    {
        Assert.Equal(expected, new CatalogFormatter(_loc).Pack(unit, packSize));
    }

    [Fact]
    public void Pack_IsTranslatedToUrdu()
    {
        _loc.SetLanguage("ur");
        var formatter = new CatalogFormatter(_loc);
        Assert.Equal("25 کلو بوری", formatter.Pack(UnitKind.Bag, 25_000));
        Assert.Equal("کھلا، فی کلو", formatter.Pack(UnitKind.Kilogram, 0));
        Assert.Equal("500 ملی لیٹر", formatter.Pack(UnitKind.Millilitre, 500));
    }

    [Theory]
    [InlineData(UnitKind.Piece, 12, "12 pcs")]
    [InlineData(UnitKind.Gram, 4_300, "4300 g")]
    [InlineData(UnitKind.Bag, 50_000, "50 kg")]
    [InlineData(UnitKind.Kilogram, 25_000, "25 kg")]
    [InlineData(UnitKind.Millilitre, 500, "500 mL")]
    public void Amount_UsesTheUnitTheItemIsCountedIn(UnitKind unit, long baseUnits, string expected)
    {
        Assert.Equal(expected, new CatalogFormatter(_loc).Amount(unit, new Quantity(baseUnits)));
    }
}
