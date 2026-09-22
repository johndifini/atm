using Atm.Domain.Accounts;

namespace Atm.Domain.Tests;

public sealed class AccountIdTests
{
    [Fact]
    public void KnownAccountsHaveStableIdentifiers()
    {
        Assert.Equal("checking", AccountId.Checking.Value);
        Assert.Equal("savings", AccountId.Savings.Value);
        Assert.NotEqual(AccountId.Checking, AccountId.Savings);
    }

    [Fact]
    public void FromNormalisesWhitespaceAndCase()
    {
        Assert.Equal(AccountId.Checking, AccountId.From("  Checking "));
        Assert.Equal(AccountId.Savings, AccountId.From("SAVINGS"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void FromRejectsBlankIdentifiers(string value)
    {
        Assert.Throws<ArgumentException>(() => AccountId.From(value));
    }

    [Fact]
    public void FromRejectsNull()
    {
        Assert.Throws<ArgumentNullException>(() => AccountId.From(null!));
    }

    [Fact]
    public void ToStringReturnsTheValue()
    {
        Assert.Equal("checking", AccountId.Checking.ToString());
    }
}
