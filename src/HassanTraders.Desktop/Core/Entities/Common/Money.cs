using System.Globalization;

namespace HassanTraders.Desktop.Core.Common;

/// <summary>Rupees stored as whole paisa. Never uses floating point or decimal.</summary>
public readonly record struct Money : IComparable<Money>
{
    public const long PaisaPerRupee = 100;

    public Money(long paisa) => Paisa = paisa;

    public static Money Zero => new(0);

    public long Paisa { get; }

    public long Rupees => Paisa / PaisaPerRupee;

    public bool IsZero => Paisa == 0;

    public bool IsNegative => Paisa < 0;

    public static Money FromRupees(long rupees, int paisa = 0)
    {
        if (paisa is < 0 or >= 100)
        {
            throw new DomainException("Paisa part must be between 0 and 99.");
        }

        return new Money(checked((rupees * PaisaPerRupee) + (rupees < 0 ? -paisa : paisa)));
    }

    public static Money operator +(Money a, Money b) => new(checked(a.Paisa + b.Paisa));

    public static Money operator -(Money a, Money b) => new(checked(a.Paisa - b.Paisa));

    public static Money operator -(Money a) => new(-a.Paisa);

    public static Money operator *(Money a, long factor) => new(checked(a.Paisa * factor));

    public static Money operator *(long factor, Money a) => a * factor;

    public static bool operator <(Money a, Money b) => a.Paisa < b.Paisa;

    public static bool operator >(Money a, Money b) => a.Paisa > b.Paisa;

    public static bool operator <=(Money a, Money b) => a.Paisa <= b.Paisa;

    public static bool operator >=(Money a, Money b) => a.Paisa >= b.Paisa;

    /// <summary>
    /// Price of <paramref name="units"/> / <paramref name="perUnits"/> of the amount, rounded half away from zero.
    /// Used for weighed goods (for example a per-kg rate times grams / 1000) with integers only.
    /// </summary>
    public Money Proportion(long units, long perUnits)
    {
        if (perUnits <= 0)
        {
            throw new DomainException("The divisor must be positive.");
        }

        var numerator = checked(Paisa * units);
        var quotient = Math.DivRem(numerator, perUnits, out var remainder);
        if (Math.Abs(remainder) * 2 >= perUnits)
        {
            quotient += numerator < 0 ? -1 : 1;
        }

        return new Money(quotient);
    }

    public int CompareTo(Money other) => Paisa.CompareTo(other.Paisa);

    /// <summary>Formats as "Rs 1,234" or "Rs 1,234.50" (invariant digits so receipts always match).</summary>
    public override string ToString()
    {
        var abs = Math.Abs(Paisa);
        var rupees = (abs / PaisaPerRupee).ToString("N0", CultureInfo.InvariantCulture);
        var paisa = abs % PaisaPerRupee;
        var sign = Paisa < 0 ? "-" : string.Empty;
        return paisa == 0
            ? $"{sign}Rs {rupees}"
            : $"{sign}Rs {rupees}.{paisa:00}";
    }
}
