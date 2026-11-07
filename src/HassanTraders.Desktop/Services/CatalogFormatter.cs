using HassanTraders.Desktop.Core.Catalog;
using HassanTraders.Desktop.Core.Common;

namespace HassanTraders.Desktop.Services;

/// <summary>
/// Turns catalogue numbers into text for the current language: "25 kg bag", "Loose per kg", "500 mL".
/// All words come from the .resx files; digits stay Latin so screens and receipts always match.
/// </summary>
public sealed class CatalogFormatter
{
    private readonly ILocalizationService _loc;

    public CatalogFormatter(ILocalizationService loc) => _loc = loc;

    public string Pack(UnitKind unit, long packSize) => unit switch
    {
        UnitKind.Bag when packSize > 0 => _loc.Format("Pack_Bag", Weight(packSize)),
        UnitKind.Bag => _loc["Unit_Bag"],
        UnitKind.Kilogram when packSize == 0 => _loc["Pack_LoosePerKg"],
        UnitKind.Kilogram or UnitKind.Gram => Weight(packSize),
        UnitKind.Millilitre or UnitKind.Litre => Volume(packSize),
        UnitKind.Dozen => _loc["Unit_Dozen"],
        UnitKind.Carton => _loc["Unit_Carton"],
        _ => _loc["Unit_Piece"],
    };

    /// <summary>A stock quantity (for example the reorder level) in the unit the item is counted in.</summary>
    public string Amount(UnitKind unit, Quantity quantity) => unit switch
    {
        UnitKind.Bag or UnitKind.Kilogram or UnitKind.Gram => Weight(quantity.BaseUnits),
        UnitKind.Millilitre or UnitKind.Litre => Volume(quantity.BaseUnits),
        _ => _loc.Format("Measure_Pieces", quantity.BaseUnits),
    };

    /// <summary>Whole kilograms are shown as kg, anything else in grams.</summary>
    public string Weight(long grams) =>
        grams != 0 && grams % Quantity.GramsPerKilogram == 0
            ? _loc.Format("Measure_Kg", grams / Quantity.GramsPerKilogram)
            : _loc.Format("Measure_G", grams);

    public string Volume(long millilitres) =>
        millilitres != 0 && millilitres % 1_000 == 0
            ? _loc.Format("Measure_L", millilitres / 1_000)
            : _loc.Format("Measure_Ml", millilitres);
}
