namespace HassanTraders.Desktop.Core.Common;

/// <summary>Whole grams for weighed goods or whole base units for counted goods.</summary>
public readonly record struct Quantity(long BaseUnits)
{
    public const long GramsPerKilogram = 1_000;
    public const long GramsPerBag = 25_000;

    public static Quantity Zero => new(0);

    public static Quantity FromGrams(long grams) => new(grams);

    public static Quantity FromKilograms(long kilograms) => new(checked(kilograms * GramsPerKilogram));

    public static Quantity FromBagsAndKilograms(long bags, long kilograms)
    {
        if (bags < 0 || kilograms is < 0 or >= 25)
        {
            throw new DomainException("Bag and kilogram quantity is invalid.");
        }

        return new Quantity(checked((bags * GramsPerBag) + (kilograms * GramsPerKilogram)));
    }

    public string BagKiloNotation
    {
        get
        {
            var bags = Math.DivRem(BaseUnits, GramsPerBag, out var remainder);
            return $"{bags}:{remainder / GramsPerKilogram}";
        }
    }

    public string FormatAsBagsAndKilos()
    {
        var bags = Math.DivRem(BaseUnits, GramsPerBag, out var remainder);
        var kilograms = Math.DivRem(remainder, GramsPerKilogram, out var grams);
        var parts = new List<string> { $"{bags} bag{(bags == 1 ? string.Empty : "s")}" };
        if (kilograms != 0)
        {
            parts.Add($"{kilograms} kg");
        }

        if (grams != 0)
        {
            parts.Add($"{grams} g");
        }

        return string.Join(" ", parts);
    }

    public static Quantity operator +(Quantity left, Quantity right) =>
        new(checked(left.BaseUnits + right.BaseUnits));

    public static Quantity operator -(Quantity left, Quantity right) =>
        new(checked(left.BaseUnits - right.BaseUnits));
}
