using HassanTraders.Desktop.Core.Common;

namespace HassanTraders.Desktop.Core.Catalog;

/// <summary>A whole-number conversion between a selling unit and its base unit.</summary>
public readonly record struct UnitConversion(UnitKind From, UnitKind To, long Factor)
{
    public long Convert(long quantity)
    {
        if (Factor <= 0)
        {
            throw new DomainException("Conversion factor must be positive.");
        }

        return checked(quantity * Factor);
    }

    public static UnitConversion BagToKilogram => new(UnitKind.Bag, UnitKind.Kilogram, 25);

    public static UnitConversion DozenToPiece => new(UnitKind.Dozen, UnitKind.Piece, 12);

    public static UnitConversion LitreToMillilitre => new(UnitKind.Litre, UnitKind.Millilitre, 1_000);
}
