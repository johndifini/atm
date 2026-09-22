using Atm.Application.Accounts;
using Atm.Application.Tests.Fakes;
using Atm.Application.Transactions;
using Atm.Domain;
using Atm.Domain.Accounts;
using Atm.Domain.Transactions;

namespace Atm.Application.Tests;

public sealed class QueryTests
{
    private readonly FakeAtmStore _store = new();

    [Fact]
    public async Task GetAccountsReturnsASummaryPerAccount()
    {
        _store.Seed(AccountId.Checking, "Checking", 1000m);
        _store.Seed(AccountId.Savings, "Savings", 250.50m);
        var query = new GetAccountsQuery(_store);

        var summaries = await query.ExecuteAsync(CancellationToken.None);

        Assert.Collection(
            summaries,
            s =>
            {
                Assert.Equal(AccountId.Checking, s.Id);
                Assert.Equal("Checking", s.Name);
                Assert.Equal(Money.From(1000m), s.Balance);
            },
            s =>
            {
                Assert.Equal(AccountId.Savings, s.Id);
                Assert.Equal("Savings", s.Name);
                Assert.Equal(Money.From(250.50m), s.Balance);
            });
    }

    [Fact]
    public async Task GetAccountsDoesNotCommit()
    {
        _store.Seed(AccountId.Checking, "Checking", 1000m);

        await new GetAccountsQuery(_store).ExecuteAsync(CancellationToken.None);

        Assert.Equal(0, _store.CommitCount);
        Assert.DoesNotContain("commit", _store.Log);
    }

    [Fact]
    public async Task GetTransactionHistoryReturnsSummariesNewestFirst()
    {
        var checking = _store.Seed(AccountId.Checking, "Checking", 1000m);
        var savings = _store.Seed(AccountId.Savings, "Savings", 1000m);
        var t1 = new DateTime(2026, 9, 21, 10, 0, 0, DateTimeKind.Utc);
        var t2 = t1.AddMinutes(1);
        var t3 = t1.AddMinutes(2);
        await _store.AddAsync(checking.Deposit(Money.From(10m), t1), CancellationToken.None);
        await _store.AddAsync(checking.TransferTo(savings, Money.From(20m), t3), CancellationToken.None);
        await _store.AddAsync(savings.Withdraw(Money.From(30m), t2), CancellationToken.None);
        var query = new GetTransactionHistoryQuery(_store);

        var history = await query.ExecuteAsync(CancellationToken.None);

        Assert.Collection(
            history,
            h =>
            {
                Assert.Equal(TransactionType.Transfer, h.Type);
                Assert.Equal(t3, h.OccurredAtUtc);
                Assert.Equal(Money.From(20m), h.Amount);
                Assert.Equal(AccountId.Checking, h.SourceAccountId);
                Assert.Equal(Money.From(990m), h.SourceBalanceAfter);
                Assert.Equal(AccountId.Savings, h.DestinationAccountId);
                Assert.Equal(Money.From(1020m), h.DestinationBalanceAfter);
            },
            h =>
            {
                Assert.Equal(TransactionType.Withdrawal, h.Type);
                Assert.Equal(t2, h.OccurredAtUtc);
                Assert.Equal(AccountId.Savings, h.SourceAccountId);
                Assert.Equal(Money.From(990m), h.SourceBalanceAfter);
                Assert.Null(h.DestinationAccountId);
            },
            h =>
            {
                Assert.Equal(TransactionType.Deposit, h.Type);
                Assert.Equal(t1, h.OccurredAtUtc);
                Assert.Null(h.SourceAccountId);
                Assert.Equal(AccountId.Checking, h.DestinationAccountId);
                Assert.Equal(Money.From(1010m), h.DestinationBalanceAfter);
            });
    }

    [Fact]
    public async Task GetTransactionHistoryIsEmptyForANewLedger()
    {
        var history = await new GetTransactionHistoryQuery(_store).ExecuteAsync(CancellationToken.None);

        Assert.Empty(history);
        Assert.Equal(0, _store.CommitCount);
    }
}
