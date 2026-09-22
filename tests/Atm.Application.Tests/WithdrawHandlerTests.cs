using Atm.Application.Errors;
using Atm.Application.Tests.Fakes;
using Atm.Application.Transactions;
using Atm.Domain;
using Atm.Domain.Accounts;
using Atm.Domain.Errors;
using Atm.Domain.Transactions;

namespace Atm.Application.Tests;

public sealed class WithdrawHandlerTests
{
    private readonly FakeAtmStore _store = new();
    private readonly FixedClock _clock = new();
    private readonly WithdrawHandler _handler;

    public WithdrawHandlerTests()
    {
        _store.Seed(AccountId.Checking, "Checking", 1000m);
        _store.Seed(AccountId.Savings, "Savings", 1000m);
        _handler = new WithdrawHandler(_store, _store, _store, _clock);
    }

    [Fact]
    public async Task WithdrawUpdatesBalanceAppendsHistoryAndCommitsOnce()
    {
        var receipt = await _handler.HandleAsync(new WithdrawCommand(AccountId.Savings, 400.40m), CancellationToken.None);

        var account = await _store.FindAsync(AccountId.Savings, CancellationToken.None);
        Assert.Equal(Money.From(599.60m), account!.Balance);

        var transaction = Assert.Single(_store.Transactions);
        Assert.Equal(TransactionType.Withdrawal, transaction.Type);
        Assert.Equal(Money.From(400.40m), transaction.Amount);
        Assert.Equal(FixedClock.Default, transaction.OccurredAtUtc);
        Assert.Equal(AccountId.Savings, transaction.SourceAccountId);
        Assert.Equal(Money.From(599.60m), transaction.SourceBalanceAfter);
        Assert.Null(transaction.DestinationAccountId);

        Assert.Equal(1, _store.CommitCount);
        Assert.Equal(transaction.Id, receipt.Id);
        Assert.Equal(Money.From(599.60m), receipt.SourceBalanceAfter);
    }

    [Fact]
    public async Task HistoryIsAppendedBeforeCommit()
    {
        await _handler.HandleAsync(new WithdrawCommand(AccountId.Checking, 1m), CancellationToken.None);

        var addIndex = _store.Log.IndexOf("add:Withdrawal");
        var commitIndex = _store.Log.IndexOf("commit");
        Assert.True(addIndex >= 0 && commitIndex > addIndex, string.Join(",", _store.Log));
    }

    [Fact]
    public async Task WithdrawingTheEntireBalanceSucceeds()
    {
        await _handler.HandleAsync(new WithdrawCommand(AccountId.Checking, 1000m), CancellationToken.None);

        var account = await _store.FindAsync(AccountId.Checking, CancellationToken.None);
        Assert.Equal(Money.Zero, account!.Balance);
        Assert.Equal(1, _store.CommitCount);
    }

    [Fact]
    public async Task OverdraftIsRejectedWithoutStateChange()
    {
        var exception = await Assert.ThrowsAsync<InsufficientFundsException>(
            () => _handler.HandleAsync(new WithdrawCommand(AccountId.Checking, 1000.01m), CancellationToken.None));

        Assert.Equal(AccountId.Checking, exception.AccountId);
        Assert.Equal(Money.From(1000m), exception.Available);
        await AssertNothingChanged();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    [InlineData(1.005)]
    public async Task InvalidAmountIsRejectedWithoutStateChange(decimal amount)
    {
        await Assert.ThrowsAsync<InvalidAmountException>(
            () => _handler.HandleAsync(new WithdrawCommand(AccountId.Checking, amount), CancellationToken.None));

        await AssertNothingChanged();
    }

    [Fact]
    public async Task UnknownAccountIsRejectedWithoutStateChange()
    {
        var unknown = AccountId.From("brokerage");

        await Assert.ThrowsAsync<AccountNotFoundException>(
            () => _handler.HandleAsync(new WithdrawCommand(unknown, 10m), CancellationToken.None));

        await AssertNothingChanged();
    }

    [Fact]
    public async Task CommitFailurePropagates()
    {
        _store.CommitFailure = new ConcurrencyConflictException();

        await Assert.ThrowsAsync<ConcurrencyConflictException>(
            () => _handler.HandleAsync(new WithdrawCommand(AccountId.Checking, 10m), CancellationToken.None));

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
