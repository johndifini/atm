using Atm.Application.Errors;
using Atm.Application.Tests.Fakes;
using Atm.Application.Transactions;
using Atm.Domain;
using Atm.Domain.Accounts;
using Atm.Domain.Errors;
using Atm.Domain.Transactions;

namespace Atm.Application.Tests;

public sealed class TransferHandlerTests
{
    private readonly FakeAtmStore _store = new();
    private readonly FixedClock _clock = new();
    private readonly TransferHandler _handler;

    public TransferHandlerTests()
    {
        _store.Seed(AccountId.Checking, "Checking", 1000m);
        _store.Seed(AccountId.Savings, "Savings", 1000m);
        _handler = new TransferHandler(_store, _store, _store, _clock);
    }

    [Fact]
    public async Task TransferMovesMoneyAppendsOneRecordAndCommitsOnce()
    {
        var receipt = await _handler.HandleAsync(
            new TransferCommand(AccountId.Checking, AccountId.Savings, 300.75m), CancellationToken.None);

        var checking = await _store.FindAsync(AccountId.Checking, CancellationToken.None);
        var savings = await _store.FindAsync(AccountId.Savings, CancellationToken.None);
        Assert.Equal(Money.From(699.25m), checking!.Balance);
        Assert.Equal(Money.From(1300.75m), savings!.Balance);

        var transaction = Assert.Single(_store.Transactions);
        Assert.Equal(TransactionType.Transfer, transaction.Type);
        Assert.Equal(Money.From(300.75m), transaction.Amount);
        Assert.Equal(FixedClock.Default, transaction.OccurredAtUtc);
        Assert.Equal(AccountId.Checking, transaction.SourceAccountId);
        Assert.Equal(Money.From(699.25m), transaction.SourceBalanceAfter);
        Assert.Equal(AccountId.Savings, transaction.DestinationAccountId);
        Assert.Equal(Money.From(1300.75m), transaction.DestinationBalanceAfter);

        Assert.Equal(1, _store.CommitCount);
        Assert.Equal(transaction.Id, receipt.Id);
        Assert.Equal(Money.From(699.25m), receipt.SourceBalanceAfter);
        Assert.Equal(Money.From(1300.75m), receipt.DestinationBalanceAfter);
    }

    [Fact]
    public async Task HistoryIsAppendedBeforeCommit()
    {
        await _handler.HandleAsync(new TransferCommand(AccountId.Savings, AccountId.Checking, 1m), CancellationToken.None);

        var addIndex = _store.Log.IndexOf("add:Transfer");
        var commitIndex = _store.Log.IndexOf("commit");
        Assert.True(addIndex >= 0 && commitIndex > addIndex, string.Join(",", _store.Log));
    }

    [Fact]
    public async Task SameAccountIsRejectedWithoutStateChange()
    {
        var exception = await Assert.ThrowsAsync<SameAccountTransferException>(
            () => _handler.HandleAsync(new TransferCommand(AccountId.Checking, AccountId.Checking, 10m), CancellationToken.None));

        Assert.Equal(AccountId.Checking, exception.AccountId);
        await AssertNothingChanged();
    }

    [Fact]
    public async Task OverdraftIsRejectedWithoutChangingEitherAccount()
    {
        var exception = await Assert.ThrowsAsync<InsufficientFundsException>(
            () => _handler.HandleAsync(new TransferCommand(AccountId.Checking, AccountId.Savings, 1000.01m), CancellationToken.None));

        Assert.Equal(AccountId.Checking, exception.AccountId);
        await AssertNothingChanged();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    [InlineData(1.005)]
    public async Task InvalidAmountIsRejectedWithoutStateChange(decimal amount)
    {
        await Assert.ThrowsAsync<InvalidAmountException>(
            () => _handler.HandleAsync(new TransferCommand(AccountId.Checking, AccountId.Savings, amount), CancellationToken.None));

        await AssertNothingChanged();
    }

    [Fact]
    public async Task UnknownSourceIsRejectedWithoutStateChange()
    {
        var unknown = AccountId.From("brokerage");

        var exception = await Assert.ThrowsAsync<AccountNotFoundException>(
            () => _handler.HandleAsync(new TransferCommand(unknown, AccountId.Savings, 10m), CancellationToken.None));

        Assert.Equal(unknown, exception.AccountId);
        await AssertNothingChanged();
    }

    [Fact]
    public async Task UnknownDestinationIsRejectedWithoutStateChange()
    {
        var unknown = AccountId.From("brokerage");

        var exception = await Assert.ThrowsAsync<AccountNotFoundException>(
            () => _handler.HandleAsync(new TransferCommand(AccountId.Checking, unknown, 10m), CancellationToken.None));

        Assert.Equal(unknown, exception.AccountId);
        await AssertNothingChanged();
    }

    [Fact]
    public async Task CommitFailurePropagates()
    {
        _store.CommitFailure = new ConcurrencyConflictException();

        await Assert.ThrowsAsync<ConcurrencyConflictException>(
            () => _handler.HandleAsync(new TransferCommand(AccountId.Checking, AccountId.Savings, 10m), CancellationToken.None));

        Assert.Equal(0, _store.CommitCount);
    }

    private async Task AssertNothingChanged()
    {
        var checking = await _store.FindAsync(AccountId.Checking, CancellationToken.None);
        var savings = await _store.FindAsync(AccountId.Savings, CancellationToken.None);
        Assert.Equal(Money.From(1000m), checking!.Balance);
        Assert.Equal(Money.From(1000m), savings!.Balance);
        Assert.Empty(_store.Transactions);
        Assert.Equal(0, _store.CommitCount);
        Assert.DoesNotContain("commit", _store.Log);
    }
}
