using HassanTraders.Desktop.Core.Common;
using HassanTraders.Desktop.Core.Catalog;

namespace HassanTraders.Desktop.Core.Tests;

public sealed class QuantityTests
{
    [Fact]
    public void BagsAndKilograms_UseWholeGramsAndFormatShopNotation()
    {
        var quantity = Quantity.FromBagsAndKilograms(5, 22) + Quantity.FromGrams(500);

        Assert.Equal(147_500, quantity.BaseUnits);
        Assert.Equal("5:22", quantity.BagKiloNotation);
        Assert.Equal("5 bags 22 kg 500 g", quantity.FormatAsBagsAndKilos());
    }

    [Fact]
    public void UnitConversions_UseIntegerFactors()
    {
        Assert.Equal(25, UnitConversion.BagToKilogram.Convert(1));
        Assert.Equal(24, UnitConversion.DozenToPiece.Convert(2));
        Assert.Equal(3_000, UnitConversion.LitreToMillilitre.Convert(3));
    }

    [Fact]
    public void BagsAndKilograms_RejectInvalidLooseWeight()
    {
        Assert.Throws<DomainException>(() => Quantity.FromBagsAndKilograms(1, 25));
    }
}
