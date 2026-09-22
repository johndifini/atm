using Atm.Domain;
using Atm.Domain.Errors;

namespace Atm.Domain.Tests;

public sealed class MoneyTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(0.01)]
    [InlineData(1)]
    [InlineData(12.34)]
    [InlineData(1000)]
    [InlineData(999999999.99)]
    public void FromAcceptsNonNegativeValuesWithAtMostTwoFractionalDigits(decimal value)
    {
        var money = Money.From(value);

        Assert.Equal(value, money.Value);
    }

    [Fact]
    public void FromAcceptsTrailingZeroFractionalDigits()
    {
        var money = Money.From(1.100m);

        Assert.Equal(1.10m, money.Value);
        Assert.Equal(Money.From(1.1m), money);
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(-1)]
    [InlineData(-1000)]
    public void FromRejectsNegativeValues(decimal value)
    {
        var exception = Assert.Throws<InvalidAmountException>(() => Money.From(value));

        Assert.Equal(value, exception.Attempted);
    }

    [Theory]
    [InlineData(0.001)]
    [InlineData(1.005)]
    [InlineData(10.123)]
    [InlineData(0.999)]
    public void FromRejectsMoreThanTwoFractionalDigits(decimal value)
    {
        var exception = Assert.Throws<InvalidAmountException>(() => Money.From(value));

        Assert.Equal(value, exception.Attempted);
    }

    [Fact]
    public void ZeroIsZero()
    {
        Assert.Equal(0m, Money.Zero.Value);
        Assert.True(Money.Zero.IsZero);
        Assert.False(Money.From(0.01m).IsZero);
    }

    [Fact]
    public void EqualityIgnoresDecimalScale()
    {
        Assert.Equal(Money.From(1.5m), Money.From(1.50m));
        Assert.True(Money.From(1.5m) == Money.From(1.50m));
        Assert.Equal(Money.From(1.5m).GetHashCode(), Money.From(1.50m).GetHashCode());
    }

    [Fact]
    public void AdditionSumsValuesExactly()
    {
        var sum = Money.From(0.10m) + Money.From(0.20m);

        Assert.Equal(Money.From(0.30m), sum);
    }

    [Fact]
    public void SubtractionProducesTheDifference()
    {
        var difference = Money.From(10.00m) - Money.From(2.50m);

        Assert.Equal(Money.From(7.50m), difference);
    }

    [Fact]
    public void SubtractionToZeroIsAllowed()
    {
        Assert.Equal(Money.Zero, Money.From(5m) - Money.From(5m));
    }

    [Fact]
    public void SubtractionBelowZeroIsImpossible()
    {
        Assert.Throws<InvalidOperationException>(() => Money.From(1m) - Money.From(1.01m));
    }

    [Fact]
    public void ComparisonOperatorsFollowValue()
    {
        var small = Money.From(1m);
        var large = Money.From(2m);

        Assert.True(small < large);
        Assert.True(small <= large);
        Assert.True(large > small);
        Assert.True(large >= small);
        Assert.True(small <= Money.From(1.00m));
        Assert.True(small >= Money.From(1.00m));
        Assert.Equal(-1, small.CompareTo(large));
    }

    [Fact]
    public void ToStringUsesTwoFractionalDigitsInvariantly()
    {
        Assert.Equal("1000.00", Money.From(1000m).ToString());
        Assert.Equal("0.50", Money.From(0.5m).ToString());
    }
}
