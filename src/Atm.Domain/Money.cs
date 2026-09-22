using System.Globalization;
using Atm.Domain.Errors;

namespace Atm.Domain;

/// <summary>
/// A non-negative monetary value with at most two fractional digits.
/// Balances and transaction amounts are both expressed as <see cref="Money"/>;
/// operations that require a strictly positive amount check <see cref="IsZero"/>.
/// </summary>
public readonly struct Money : IEquatable<Money>, IComparable<Money>
{
    public const int FractionalDigits = 2;

    public static readonly Money Zero = default;

    private Money(decimal value)
    {
        Value = value;
    }

    public decimal Value { get; }

    public bool IsZero => Value == 0m;

    public static Money From(decimal value)
    {
        if (value < 0m)
        {
            throw new InvalidAmountException(value, "Amount cannot be negative.");
        }

        var rounded = decimal.Round(value, FractionalDigits);
        if (rounded != value)
        {
            throw new InvalidAmountException(value, "Amount cannot have more than two fractional digits.");
        }

        return new Money(rounded);
    }

    public static Money operator +(Money left, Money right) => new(left.Value + right.Value);

    public static Money operator -(Money left, Money right)
    {
        if (right.Value > left.Value)
        {
            throw new InvalidOperationException("Money cannot be negative.");
        }

        return new Money(left.Value - right.Value);
    }

    public static bool operator ==(Money left, Money right) => left.Equals(right);

    public static bool operator !=(Money left, Money right) => !left.Equals(right);

    public static bool operator <(Money left, Money right) => left.Value < right.Value;

    public static bool operator <=(Money left, Money right) => left.Value <= right.Value;

    public static bool operator >(Money left, Money right) => left.Value > right.Value;

    public static bool operator >=(Money left, Money right) => left.Value >= right.Value;

    public int CompareTo(Money other) => Value.CompareTo(other.Value);

    public bool Equals(Money other) => Value == other.Value;

    public override bool Equals(object? obj) => obj is Money other && Equals(other);

    public override int GetHashCode() => Value.GetHashCode();

    public override string ToString() => Value.ToString("0.00", CultureInfo.InvariantCulture);
}
