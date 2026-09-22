namespace Atm.Domain.Accounts;

/// <summary>
/// Stable, case-insensitive identifier of an account. The two accounts in this
/// system are <see cref="Checking"/> and <see cref="Savings"/>.
/// </summary>
public readonly struct AccountId : IEquatable<AccountId>
{
    public static readonly AccountId Checking = new("checking");

    public static readonly AccountId Savings = new("savings");

    private AccountId(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static AccountId From(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var normalised = value.Trim().ToLowerInvariant();
        if (normalised.Length == 0)
        {
            throw new ArgumentException("Account identifier cannot be blank.", nameof(value));
        }

        return new AccountId(normalised);
    }

    public static bool operator ==(AccountId left, AccountId right) => left.Equals(right);

    public static bool operator !=(AccountId left, AccountId right) => !left.Equals(right);

    public bool Equals(AccountId other) => string.Equals(Value, other.Value, StringComparison.Ordinal);

    public override bool Equals(object? obj) => obj is AccountId other && Equals(other);

    public override int GetHashCode() => Value is null ? 0 : StringComparer.Ordinal.GetHashCode(Value);

    public override string ToString() => Value;
}
