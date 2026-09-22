using Atm.Domain;
using Atm.Domain.Accounts;
using Atm.Domain.Errors;
using Atm.Domain.Transactions;

namespace Atm.Domain.Tests;

public sealed class AccountTests
{
    private static readonly DateTime Now = new(2026, 9, 21, 12, 0, 0, DateTimeKind.Utc);

    private static Account Checking(decimal balance = 1000m) =>
        new(AccountId.Checking, "Checking", Money.From(balance));

    [Fact]
    public void ConstructorSetsIdentityNameAndOpeningBalance()
    {
        var account = new Account(AccountId.Savings, "Savings", Money.From(1000m));

        Assert.Equal(AccountId.Savings, account.Id);
        Assert.Equal("Savings", account.Name);
        Assert.Equal(Money.From(1000m), account.Balance);
        Assert.Equal(0, account.Version);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void ConstructorRejectsBlankName(string name)
    {
        Assert.Throws<ArgumentException>(() => new Account(AccountId.Checking, name, Money.Zero));
    }

    [Fact]
    public void ConstructorRejectsNullName()
    {
        Assert.Throws<ArgumentNullException>(() => new Account(AccountId.Checking, null!, Money.Zero));
    }

    [Fact]
    public void ConstructorTrimsName()
    {
        var account = new Account(AccountId.Checking, "  Checking  ", Money.Zero);

        Assert.Equal("Checking", account.Name);
    }

    // Deposits

    [Fact]
    public void DepositIncreasesBalance()
    {
        var account = Checking();

        account.Deposit(Money.From(250.25m), Now);

        Assert.Equal(Money.From(1250.25m), account.Balance);
    }

    [Fact]
    public void DepositRecordsHistoryAgainstTheDestination()
    {
        var account = Checking();

        var transaction = account.Deposit(Money.From(250.25m), Now);

        Assert.NotEqual(Guid.Empty, transaction.Id);
        Assert.Equal(TransactionType.Deposit, transaction.Type);
        Assert.Equal(Now, transaction.OccurredAtUtc);
        Assert.Equal(Money.From(250.25m), transaction.Amount);
        Assert.Null(transaction.SourceAccountId);
        Assert.Null(transaction.SourceBalanceAfter);
        Assert.Equal(AccountId.Checking, transaction.DestinationAccountId);
        Assert.Equal(Money.From(1250.25m), transaction.DestinationBalanceAfter);
    }

    [Fact]
    public void DepositIncrementsVersion()
    {
        var account = Checking();

        account.Deposit(Money.From(1m), Now);
        account.Deposit(Money.From(1m), Now);

        Assert.Equal(2, account.Version);
    }

    [Fact]
    public void DepositRejectsZeroWithoutMutating()
    {
        var account = Checking();

        Assert.Throws<InvalidAmountException>(() => account.Deposit(Money.Zero, Now));

        Assert.Equal(Money.From(1000m), account.Balance);
        Assert.Equal(0, account.Version);
    }

    [Theory]
    [InlineData(DateTimeKind.Local)]
    [InlineData(DateTimeKind.Unspecified)]
    public void DepositRejectsNonUtcTimestampsWithoutMutating(DateTimeKind kind)
    {
        var account = Checking();
        var timestamp = new DateTime(2026, 9, 21, 12, 0, 0, kind);

        Assert.Throws<ArgumentException>(() => account.Deposit(Money.From(1m), timestamp));

        Assert.Equal(Money.From(1000m), account.Balance);
        Assert.Equal(0, account.Version);
    }

    // Withdrawals

    [Fact]
    public void WithdrawDecreasesBalance()
    {
        var account = Checking();

        account.Withdraw(Money.From(400.40m), Now);

        Assert.Equal(Money.From(599.60m), account.Balance);
    }

    [Fact]
    public void WithdrawRecordsHistoryAgainstTheSource()
    {
        var account = Checking();

        var transaction = account.Withdraw(Money.From(400.40m), Now);

        Assert.NotEqual(Guid.Empty, transaction.Id);
        Assert.Equal(TransactionType.Withdrawal, transaction.Type);
        Assert.Equal(Now, transaction.OccurredAtUtc);
        Assert.Equal(Money.From(400.40m), transaction.Amount);
        Assert.Equal(AccountId.Checking, transaction.SourceAccountId);
        Assert.Equal(Money.From(599.60m), transaction.SourceBalanceAfter);
        Assert.Null(transaction.DestinationAccountId);
        Assert.Null(transaction.DestinationBalanceAfter);
    }

    [Fact]
    public void WithdrawingTheEntireBalanceIsAllowed()
    {
        var account = Checking();

        account.Withdraw(Money.From(1000m), Now);

        Assert.Equal(Money.Zero, account.Balance);
        Assert.Equal(1, account.Version);
    }

    [Fact]
    public void WithdrawRejectsOverdraftWithoutMutating()
    {
        var account = Checking();

        var exception = Assert.Throws<InsufficientFundsException>(
            () => account.Withdraw(Money.From(1000.01m), Now));

        Assert.Equal(AccountId.Checking, exception.AccountId);
        Assert.Equal(Money.From(1000m), exception.Available);
        Assert.Equal(Money.From(1000.01m), exception.Requested);
        Assert.Equal(Money.From(1000m), account.Balance);
        Assert.Equal(0, account.Version);
    }

    [Fact]
    public void WithdrawFromEmptyAccountIsRejected()
    {
        var account = Checking(0m);

        Assert.Throws<InsufficientFundsException>(() => account.Withdraw(Money.From(0.01m), Now));

        Assert.Equal(Money.Zero, account.Balance);
    }

    [Fact]
    public void WithdrawRejectsZeroWithoutMutating()
    {
        var account = Checking();

        Assert.Throws<InvalidAmountException>(() => account.Withdraw(Money.Zero, Now));

        Assert.Equal(Money.From(1000m), account.Balance);
        Assert.Equal(0, account.Version);
    }

    [Theory]
    [InlineData(DateTimeKind.Local)]
    [InlineData(DateTimeKind.Unspecified)]
    public void WithdrawRejectsNonUtcTimestampsWithoutMutating(DateTimeKind kind)
    {
        var account = Checking();
        var timestamp = new DateTime(2026, 9, 21, 12, 0, 0, kind);

        Assert.Throws<ArgumentException>(() => account.Withdraw(Money.From(1m), timestamp));

        Assert.Equal(Money.From(1000m), account.Balance);
        Assert.Equal(0, account.Version);
    }

    [Fact]
    public void RepeatedOperationsNeverProduceANegativeBalance()
    {
        var account = Checking(10m);

        account.Withdraw(Money.From(4m), Now);
        account.Withdraw(Money.From(6m), Now);
        Assert.Throws<InsufficientFundsException>(() => account.Withdraw(Money.From(0.01m), Now));

        Assert.Equal(Money.Zero, account.Balance);
        Assert.Equal(2, account.Version);
    }
}
