using Atm.Application.Errors;
using Atm.Application.Tests.Fakes;
using Atm.Application.Transactions;
using Atm.Domain;
using Atm.Domain.Accounts;
using Atm.Domain.Errors;
using Atm.Domain.Transactions;

namespace Atm.Application.Tests;

public sealed class DepositHandlerTests
{
    private readonly FakeAtmStore _store = new();
    private readonly FixedClock _clock = new();
    private readonly DepositHandler _handler;

    public DepositHandlerTests()
    {
        _store.Seed(AccountId.Checking, "Checking", 1000m);
        _store.Seed(AccountId.Savings, "Savings", 1000m);
        _handler = new DepositHandler(_store, _store, _store, _clock);
    }

    [Fact]
    public async Task DepositUpdatesBalanceAppendsHistoryAndCommitsOnce()
    {
        var receipt = await _handler.HandleAsync(new DepositCommand(AccountId.Checking, 250.25m), CancellationToken.None);

        var account = await _store.FindAsync(AccountId.Checking, CancellationToken.None);
        Assert.Equal(Money.From(1250.25m), account!.Balance);

        var transaction = Assert.Single(_store.Transactions);
        Assert.Equal(TransactionType.Deposit, transaction.Type);
        Assert.Equal(Money.From(250.25m), transaction.Amount);
        Assert.Equal(FixedClock.Default, transaction.OccurredAtUtc);
        Assert.Equal(AccountId.Checking, transaction.DestinationAccountId);
        Assert.Equal(Money.From(1250.25m), transaction.DestinationBalanceAfter);

        Assert.Equal(1, _store.CommitCount);
        Assert.Equal(transaction.Id, receipt.Id);
        Assert.Equal(TransactionType.Deposit, receipt.Type);
        Assert.Equal(Money.From(250.25m), receipt.Amount);
        Assert.Equal(Money.From(1250.25m), receipt.DestinationBalanceAfter);
    }

    [Fact]
    public async Task HistoryIsAppendedBeforeCommit()
    {
        await _handler.HandleAsync(new DepositCommand(AccountId.Savings, 1m), CancellationToken.None);

        var addIndex = _store.Log.IndexOf("add:Deposit");
        var commitIndex = _store.Log.IndexOf("commit");
        Assert.True(addIndex >= 0 && commitIndex > addIndex, string.Join(",", _store.Log));
    }

    [Fact]
    public async Task DepositUsesTheClockForTheTimestamp()
    {
        _clock.UtcNow = new DateTime(2030, 1, 2, 3, 4, 5, DateTimeKind.Utc);

        var receipt = await _handler.HandleAsync(new DepositCommand(AccountId.Checking, 1m), CancellationToken.None);

        Assert.Equal(_clock.UtcNow, receipt.OccurredAtUtc);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    [InlineData(1.005)]
    public async Task InvalidAmountIsRejectedWithoutStateChange(decimal amount)
    {
        await Assert.ThrowsAsync<InvalidAmountException>(
            () => _handler.HandleAsync(new DepositCommand(AccountId.Checking, amount), CancellationToken.None));

        await AssertNothingChanged();
    }

    [Fact]
    public async Task UnknownAccountIsRejectedWithoutStateChange()
    {
        var unknown = AccountId.From("brokerage");

        var exception = await Assert.ThrowsAsync<AccountNotFoundException>(
            () => _handler.HandleAsync(new DepositCommand(unknown, 10m), CancellationToken.None));

        Assert.Equal(unknown, exception.AccountId);
        await AssertNothingChanged();
    }

    [Fact]
    public async Task CommitFailurePropagates()
    {
        _store.CommitFailure = new ConcurrencyConflictException();

        await Assert.ThrowsAsync<ConcurrencyConflictException>(
            () => _handler.HandleAsync(new DepositCommand(AccountId.Checking, 10m), CancellationToken.None));

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
