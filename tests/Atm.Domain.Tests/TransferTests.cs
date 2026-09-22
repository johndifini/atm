using Atm.Domain;
using Atm.Domain.Accounts;
using Atm.Domain.Errors;
using Atm.Domain.Transactions;

namespace Atm.Domain.Tests;

public sealed class TransferTests
{
    private static readonly DateTime Now = new(2026, 9, 21, 12, 0, 0, DateTimeKind.Utc);

    private static Account Checking(decimal balance = 1000m) =>
        new(AccountId.Checking, "Checking", Money.From(balance));

    private static Account Savings(decimal balance = 1000m) =>
        new(AccountId.Savings, "Savings", Money.From(balance));

    [Fact]
    public void TransferMovesMoneyBetweenAccounts()
    {
        var checking = Checking();
        var savings = Savings();

        checking.TransferTo(savings, Money.From(300.75m), Now);

        Assert.Equal(Money.From(699.25m), checking.Balance);
        Assert.Equal(Money.From(1300.75m), savings.Balance);
    }

    [Fact]
    public void TransferPreservesTotalMoney()
    {
        var checking = Checking(123.45m);
        var savings = Savings(678.90m);

        checking.TransferTo(savings, Money.From(100m), Now);

        Assert.Equal(Money.From(802.35m), checking.Balance + savings.Balance);
    }

    [Fact]
    public void TransferRecordsBothAccountsAndBothResultingBalances()
    {
        var checking = Checking();
        var savings = Savings();

        var transaction = checking.TransferTo(savings, Money.From(300.75m), Now);

        Assert.NotEqual(Guid.Empty, transaction.Id);
        Assert.Equal(TransactionType.Transfer, transaction.Type);
        Assert.Equal(Now, transaction.OccurredAtUtc);
        Assert.Equal(Money.From(300.75m), transaction.Amount);
        Assert.Equal(AccountId.Checking, transaction.SourceAccountId);
        Assert.Equal(Money.From(699.25m), transaction.SourceBalanceAfter);
        Assert.Equal(AccountId.Savings, transaction.DestinationAccountId);
        Assert.Equal(Money.From(1300.75m), transaction.DestinationBalanceAfter);
    }

    [Fact]
    public void TransferIncrementsBothVersions()
    {
        var checking = Checking();
        var savings = Savings();

        checking.TransferTo(savings, Money.From(1m), Now);

        Assert.Equal(1, checking.Version);
        Assert.Equal(1, savings.Version);
    }

    [Fact]
    public void TransferOfTheEntireBalanceIsAllowed()
    {
        var checking = Checking();
        var savings = Savings();

        checking.TransferTo(savings, Money.From(1000m), Now);

        Assert.Equal(Money.Zero, checking.Balance);
        Assert.Equal(Money.From(2000m), savings.Balance);
    }

    [Fact]
    public void TransferRejectsSameAccountWithoutMutating()
    {
        var checking = Checking();

        var exception = Assert.Throws<SameAccountTransferException>(
            () => checking.TransferTo(checking, Money.From(1m), Now));

        Assert.Equal(AccountId.Checking, exception.AccountId);
        Assert.Equal(Money.From(1000m), checking.Balance);
        Assert.Equal(0, checking.Version);
    }

    [Fact]
    public void TransferRejectsSameAccountIdentityAcrossInstances()
    {
        var checking = Checking();
        var anotherChecking = Checking();

        Assert.Throws<SameAccountTransferException>(
            () => checking.TransferTo(anotherChecking, Money.From(1m), Now));

        Assert.Equal(Money.From(1000m), checking.Balance);
        Assert.Equal(Money.From(1000m), anotherChecking.Balance);
    }

    [Fact]
    public void TransferRejectsOverdraftWithoutMutatingEitherAccount()
    {
        var checking = Checking(50m);
        var savings = Savings();

        var exception = Assert.Throws<InsufficientFundsException>(
            () => checking.TransferTo(savings, Money.From(50.01m), Now));

        Assert.Equal(AccountId.Checking, exception.AccountId);
        Assert.Equal(Money.From(50m), exception.Available);
        Assert.Equal(Money.From(50.01m), exception.Requested);
        Assert.Equal(Money.From(50m), checking.Balance);
        Assert.Equal(Money.From(1000m), savings.Balance);
        Assert.Equal(0, checking.Version);
        Assert.Equal(0, savings.Version);
    }

    [Fact]
    public void TransferRejectsZeroWithoutMutating()
    {
        var checking = Checking();
        var savings = Savings();

        Assert.Throws<InvalidAmountException>(() => checking.TransferTo(savings, Money.Zero, Now));

        Assert.Equal(Money.From(1000m), checking.Balance);
        Assert.Equal(Money.From(1000m), savings.Balance);
        Assert.Equal(0, checking.Version);
        Assert.Equal(0, savings.Version);
    }

    [Fact]
    public void TransferRejectsNullDestination()
    {
        var checking = Checking();

        Assert.Throws<ArgumentNullException>(() => checking.TransferTo(null!, Money.From(1m), Now));

        Assert.Equal(Money.From(1000m), checking.Balance);
    }

    [Theory]
    [InlineData(DateTimeKind.Local)]
    [InlineData(DateTimeKind.Unspecified)]
    public void TransferRejectsNonUtcTimestampsWithoutMutating(DateTimeKind kind)
    {
        var checking = Checking();
        var savings = Savings();
        var timestamp = new DateTime(2026, 9, 21, 12, 0, 0, kind);

        Assert.Throws<ArgumentException>(() => checking.TransferTo(savings, Money.From(1m), timestamp));

        Assert.Equal(Money.From(1000m), checking.Balance);
        Assert.Equal(Money.From(1000m), savings.Balance);
    }
}
