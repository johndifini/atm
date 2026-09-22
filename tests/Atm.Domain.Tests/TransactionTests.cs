using Atm.Domain;
using Atm.Domain.Accounts;
using Atm.Domain.Transactions;

namespace Atm.Domain.Tests;

public sealed class TransactionTests
{
    private static readonly DateTime Now = new(2026, 9, 21, 12, 0, 0, DateTimeKind.Utc);
    private static readonly Guid Id = Guid.NewGuid();
    private static readonly Money Amount = Money.From(10m);
    private static readonly Money Balance = Money.From(100m);

    [Fact]
    public void DepositRequiresOnlyADestination()
    {
        var transaction = new Transaction(
            Id, Now, TransactionType.Deposit, Amount,
            sourceAccountId: null, sourceBalanceAfter: null,
            destinationAccountId: AccountId.Checking, destinationBalanceAfter: Balance);

        Assert.Equal(Id, transaction.Id);
        Assert.Equal(AccountId.Checking, transaction.DestinationAccountId);
    }

    [Fact]
    public void WithdrawalRequiresOnlyASource()
    {
        var transaction = new Transaction(
            Id, Now, TransactionType.Withdrawal, Amount,
            sourceAccountId: AccountId.Checking, sourceBalanceAfter: Balance,
            destinationAccountId: null, destinationBalanceAfter: null);

        Assert.Equal(AccountId.Checking, transaction.SourceAccountId);
    }

    [Fact]
    public void TransferRequiresBothSides()
    {
        var transaction = new Transaction(
            Id, Now, TransactionType.Transfer, Amount,
            sourceAccountId: AccountId.Checking, sourceBalanceAfter: Balance,
            destinationAccountId: AccountId.Savings, destinationBalanceAfter: Balance);

        Assert.Equal(AccountId.Checking, transaction.SourceAccountId);
        Assert.Equal(AccountId.Savings, transaction.DestinationAccountId);
    }

    [Fact]
    public void EmptyIdIsRejected()
    {
        Assert.Throws<ArgumentException>(() => new Transaction(
            Guid.Empty, Now, TransactionType.Deposit, Amount,
            null, null, AccountId.Checking, Balance));
    }

    [Fact]
    public void ZeroAmountIsRejected()
    {
        Assert.Throws<ArgumentException>(() => new Transaction(
            Id, Now, TransactionType.Deposit, Money.Zero,
            null, null, AccountId.Checking, Balance));
    }

    [Theory]
    [InlineData(DateTimeKind.Local)]
    [InlineData(DateTimeKind.Unspecified)]
    public void NonUtcTimestampIsRejected(DateTimeKind kind)
    {
        var timestamp = new DateTime(2026, 9, 21, 12, 0, 0, kind);

        Assert.Throws<ArgumentException>(() => new Transaction(
            Id, timestamp, TransactionType.Deposit, Amount,
            null, null, AccountId.Checking, Balance));
    }

    [Fact]
    public void DepositWithASourceIsRejected()
    {
        Assert.Throws<ArgumentException>(() => new Transaction(
            Id, Now, TransactionType.Deposit, Amount,
            AccountId.Savings, Balance, AccountId.Checking, Balance));
    }

    [Fact]
    public void DepositWithoutADestinationIsRejected()
    {
        Assert.Throws<ArgumentException>(() => new Transaction(
            Id, Now, TransactionType.Deposit, Amount,
            null, null, null, null));
    }

    [Fact]
    public void WithdrawalWithADestinationIsRejected()
    {
        Assert.Throws<ArgumentException>(() => new Transaction(
            Id, Now, TransactionType.Withdrawal, Amount,
            AccountId.Checking, Balance, AccountId.Savings, Balance));
    }

    [Fact]
    public void TransferMissingADestinationIsRejected()
    {
        Assert.Throws<ArgumentException>(() => new Transaction(
            Id, Now, TransactionType.Transfer, Amount,
            AccountId.Checking, Balance, null, null));
    }

    [Fact]
    public void TransferBetweenTheSameAccountIsRejected()
    {
        Assert.Throws<ArgumentException>(() => new Transaction(
            Id, Now, TransactionType.Transfer, Amount,
            AccountId.Checking, Balance, AccountId.Checking, Balance));
    }

    [Fact]
    public void AccountIdAndBalanceAfterMustBePairedOnEachSide()
    {
        Assert.Throws<ArgumentException>(() => new Transaction(
            Id, Now, TransactionType.Deposit, Amount,
            null, null, AccountId.Checking, null));

        Assert.Throws<ArgumentException>(() => new Transaction(
            Id, Now, TransactionType.Withdrawal, Amount,
            AccountId.Checking, null, null, null));
    }

    [Fact]
    public void UnknownTypeIsRejected()
    {
        Assert.Throws<ArgumentException>(() => new Transaction(
            Id, Now, (TransactionType)99, Amount,
            null, null, AccountId.Checking, Balance));
    }
}
