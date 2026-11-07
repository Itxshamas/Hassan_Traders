using HassanTraders.Desktop.Core.Common;

namespace HassanTraders.Desktop.Core.Tests;

public class MoneyTests
{
    [Fact]
    public void FromRupees_StoresWholePaisa()
    {
        Assert.Equal(12_550, Money.FromRupees(125, 50).Paisa);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(100)]
    public void FromRupees_RejectsInvalidPaisaPart(int paisa)
    {
        Assert.Throws<DomainException>(() => Money.FromRupees(1, paisa));
    }

    [Fact]
    public void Arithmetic_IsExact()
    {
        var total = Money.FromRupees(20) + Money.FromRupees(0, 10) + Money.FromRupees(0, 20);
        Assert.Equal(new Money(2_030), total);
        Assert.Equal(new Money(6_000), Money.FromRupees(20) * 3);
        Assert.Equal(new Money(-500), Money.FromRupees(5) - Money.FromRupees(10));
    }

    [Fact]
    public void Arithmetic_OverflowThrows()
    {
        Assert.Throws<OverflowException>(() => new Money(long.MaxValue) + new Money(1));
    }

    [Theory]
    [InlineData(25_000, 22_000, 1_000, 550_000)] // Rs 250/kg * 22 kg
    [InlineData(25_000, 500, 1_000, 125_00)]    // Rs 250/kg * 500 g
    [InlineData(1_000, 1, 3, 333)]              // rounds down
    [InlineData(1_000, 2, 3, 667)]              // rounds up
    [InlineData(1, 1, 2, 1)]                    // half rounds away from zero
    public void Proportion_RoundsHalfAwayFromZero(long paisa, long units, long perUnits, long expected)
    {
        Assert.Equal(expected, new Money(paisa).Proportion(units, perUnits).Paisa);
    }

    [Fact]
    public void Proportion_RejectsNonPositiveDivisor()
    {
        Assert.Throws<DomainException>(() => Money.FromRupees(1).Proportion(1, 0));
    }

    [Theory]
    [InlineData(0, "Rs 0")]
    [InlineData(2_000, "Rs 20")]
    [InlineData(123_456_50, "Rs 123,456.50")]
    [InlineData(5, "Rs 0.05")]
    [InlineData(-1_050, "-Rs 10.50")]
    public void ToString_UsesInvariantGrouping(long paisa, string expected)
    {
        Assert.Equal(expected, new Money(paisa).ToString());
    }

    [Fact]
    public void Comparison_Works()
    {
        Assert.True(Money.FromRupees(5) < Money.FromRupees(6));
        Assert.True(Money.Zero.IsZero);
        Assert.True(new Money(-1).IsNegative);
    }
}
