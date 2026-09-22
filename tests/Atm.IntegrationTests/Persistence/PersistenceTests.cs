using Atm.Application.Errors;
using Atm.Application.Ports;
using Atm.Application.Transactions;
using Atm.Domain;
using Atm.Domain.Accounts;
using Atm.Domain.Transactions;
using Atm.Infrastructure;
using Atm.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Atm.IntegrationTests.Persistence;

/// <summary>
/// Drives the real application handlers through the SQLite adapters, using a
/// fresh service scope per operation the way the web host would per request.
/// </summary>
public sealed class PersistenceTests : IAsyncLifetime, IDisposable
{
    private static readonly DateTime T0 = new(2026, 9, 21, 9, 0, 0, DateTimeKind.Utc);

    private readonly SqliteDatabase _database = new();
    private readonly MutableClock _clock = new() { UtcNow = T0 };
    private ServiceProvider _services = null!;

    public async Task InitializeAsync()
    {
        await _database.InitializeAsync();

        var services = new ServiceCollection();
        services.AddAtmPersistence(_database.ConnectionString);
        services.AddSingleton<IClock>(_clock);
        services.AddScoped<DepositHandler>();
        services.AddScoped<WithdrawHandler>();
        services.AddScoped<TransferHandler>();
        services.AddScoped<GetTransactionHistoryQuery>();
        _services = services.BuildServiceProvider();
    }

    public Task DisposeAsync() => _services.DisposeAsync().AsTask();

    public void Dispose() => _database.Dispose();

    [Fact]
    public async Task DepositPersistsBalanceAndHistoryWithUtcTimestamp()
    {
        await using (var scope = _services.CreateAsyncScope())
        {
            var handler = scope.ServiceProvider.GetRequiredService<DepositHandler>();
            await handler.HandleAsync(new DepositCommand(AccountId.Checking, 250.25m), CancellationToken.None);
        }

        await using var db = _database.CreateContext();
        var checking = await db.Accounts.SingleAsync(a => a.Id == AccountId.Checking);
        Assert.Equal(Money.From(1250.25m), checking.Balance);
        Assert.Equal(1, checking.Version);

        var record = await db.Transactions.SingleAsync();
        Assert.Equal(TransactionType.Deposit, record.Type);
        Assert.Equal(Money.From(250.25m), record.Amount);
        Assert.Equal(T0, record.OccurredAtUtc);
        Assert.Equal(DateTimeKind.Utc, record.OccurredAtUtc.Kind);
        Assert.Null(record.SourceAccountId);
        Assert.Null(record.SourceBalanceAfter);
        Assert.Equal(AccountId.Checking, record.DestinationAccountId);
        Assert.Equal(Money.From(1250.25m), record.DestinationBalanceAfter);
    }

    [Fact]
    public async Task TransferPersistsBothAccountsAndOneRecord()
    {
        await using (var scope = _services.CreateAsyncScope())
        {
            var handler = scope.ServiceProvider.GetRequiredService<TransferHandler>();
            await handler.HandleAsync(
                new TransferCommand(AccountId.Savings, AccountId.Checking, 300.75m), CancellationToken.None);
        }

        await using var db = _database.CreateContext();
        var checking = await db.Accounts.SingleAsync(a => a.Id == AccountId.Checking);
        var savings = await db.Accounts.SingleAsync(a => a.Id == AccountId.Savings);
        Assert.Equal(Money.From(1300.75m), checking.Balance);
        Assert.Equal(Money.From(699.25m), savings.Balance);
        Assert.Equal(1, checking.Version);
        Assert.Equal(1, savings.Version);

        var record = await db.Transactions.SingleAsync();
        Assert.Equal(TransactionType.Transfer, record.Type);
        Assert.Equal(AccountId.Savings, record.SourceAccountId);
        Assert.Equal(Money.From(699.25m), record.SourceBalanceAfter);
        Assert.Equal(AccountId.Checking, record.DestinationAccountId);
        Assert.Equal(Money.From(1300.75m), record.DestinationBalanceAfter);
    }

    [Fact]
    public async Task OverdraftLeavesTheDatabaseUntouched()
    {
        await using (var scope = _services.CreateAsyncScope())
        {
            var handler = scope.ServiceProvider.GetRequiredService<WithdrawHandler>();
            await Assert.ThrowsAsync<Domain.Errors.InsufficientFundsException>(
                () => handler.HandleAsync(new WithdrawCommand(AccountId.Checking, 1000.01m), CancellationToken.None));
        }

        await using var db = _database.CreateContext();
        var checking = await db.Accounts.SingleAsync(a => a.Id == AccountId.Checking);
        Assert.Equal(Money.From(1000m), checking.Balance);
        Assert.Equal(0, checking.Version);
        Assert.Empty(await db.Transactions.ToListAsync());
    }

    [Fact]
    public async Task HistoryIsReturnedNewestFirstAcrossRestarts()
    {
        await RunAsync<DepositHandler, DepositCommand>(
            (h, c) => h.HandleAsync(c, CancellationToken.None), new DepositCommand(AccountId.Checking, 10m));
        _clock.UtcNow = T0.AddMinutes(1);
        await RunAsync<WithdrawHandler, WithdrawCommand>(
            (h, c) => h.HandleAsync(c, CancellationToken.None), new WithdrawCommand(AccountId.Savings, 20m));
        _clock.UtcNow = T0.AddMinutes(2);
        await RunAsync<TransferHandler, TransferCommand>(
            (h, c) => h.HandleAsync(c, CancellationToken.None), new TransferCommand(AccountId.Checking, AccountId.Savings, 30m));

        // A brand-new provider over the same file stands in for an application restart.
        var restarted = new ServiceCollection()
            .AddAtmPersistence(_database.ConnectionString)
            .AddScoped<GetTransactionHistoryQuery>()
            .BuildServiceProvider();
        await using (restarted)
        {
            await using var scope = restarted.CreateAsyncScope();
            var query = scope.ServiceProvider.GetRequiredService<GetTransactionHistoryQuery>();
            var history = await query.ExecuteAsync(CancellationToken.None);

            Assert.Collection(
                history,
                h => Assert.Equal(TransactionType.Transfer, h.Type),
                h => Assert.Equal(TransactionType.Withdrawal, h.Type),
                h => Assert.Equal(TransactionType.Deposit, h.Type));
            Assert.All(history, h => Assert.Equal(DateTimeKind.Utc, h.OccurredAtUtc.Kind));
        }
    }

    [Fact]
    public async Task RecordsWithTheSameTimestampAreOrderedByIdNewestFirst()
    {
        // Same clock value for all three; version-7 GUIDs still increase.
        var first = await RunAsync<DepositHandler, DepositCommand>(
            (h, c) => h.HandleAsync(c, CancellationToken.None), new DepositCommand(AccountId.Checking, 1m));
        var second = await RunAsync<DepositHandler, DepositCommand>(
            (h, c) => h.HandleAsync(c, CancellationToken.None), new DepositCommand(AccountId.Checking, 2m));
        var third = await RunAsync<DepositHandler, DepositCommand>(
            (h, c) => h.HandleAsync(c, CancellationToken.None), new DepositCommand(AccountId.Checking, 3m));

        await using var scope = _services.CreateAsyncScope();
        var history = await scope.ServiceProvider.GetRequiredService<GetTransactionHistoryQuery>()
            .ExecuteAsync(CancellationToken.None);

        Assert.Equal([third.Id, second.Id, first.Id], history.Select(h => h.Id).ToArray());
    }

    [Fact]
    public async Task ConcurrentUpdatesToTheSameAccountAreDetectedAndTheLoserWritesNothing()
    {
        await using var scopeA = _services.CreateAsyncScope();
        await using var scopeB = _services.CreateAsyncScope();
        var handlerA = scopeA.ServiceProvider.GetRequiredService<WithdrawHandler>();
        var handlerB = scopeB.ServiceProvider.GetRequiredService<WithdrawHandler>();

        // Both requests load the account at Version 0 before either commits.
        var accountsA = scopeA.ServiceProvider.GetRequiredService<IAccountRepository>();
        var accountsB = scopeB.ServiceProvider.GetRequiredService<IAccountRepository>();
        Assert.NotNull(await accountsA.FindAsync(AccountId.Checking, CancellationToken.None));
        Assert.NotNull(await accountsB.FindAsync(AccountId.Checking, CancellationToken.None));

        await handlerA.HandleAsync(new WithdrawCommand(AccountId.Checking, 600m), CancellationToken.None);
        await Assert.ThrowsAsync<ConcurrencyConflictException>(
            () => handlerB.HandleAsync(new WithdrawCommand(AccountId.Checking, 600m), CancellationToken.None));

        await using var db = _database.CreateContext();
        var checking = await db.Accounts.SingleAsync(a => a.Id == AccountId.Checking);
        Assert.Equal(Money.From(400m), checking.Balance);
        Assert.Equal(1, checking.Version);
        Assert.Equal(1, await db.Transactions.CountAsync());
    }

    [Fact]
    public async Task BalanceChangeAndHistoryCommitTogetherOrNotAtAll()
    {
        // Seed one record, then attempt a second commit that mutates an account
        // but tries to append a history row with a duplicate primary key.
        var existing = await RunAsync<DepositHandler, DepositCommand>(
            (h, c) => h.HandleAsync(c, CancellationToken.None), new DepositCommand(AccountId.Checking, 5m));

        await using (var scope = _services.CreateAsyncScope())
        {
            var accounts = scope.ServiceProvider.GetRequiredService<IAccountRepository>();
            var transactions = scope.ServiceProvider.GetRequiredService<ITransactionRepository>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            var savings = (await accounts.FindAsync(AccountId.Savings, CancellationToken.None))!;
            var real = savings.Withdraw(Money.From(100m), T0);
            var duplicate = new Transaction(
                existing.Id, real.OccurredAtUtc, real.Type, real.Amount,
                real.SourceAccountId, real.SourceBalanceAfter, real.DestinationAccountId, real.DestinationBalanceAfter);
            await transactions.AddAsync(duplicate, CancellationToken.None);

            await Assert.ThrowsAnyAsync<DbUpdateException>(() => unitOfWork.CommitAsync(CancellationToken.None));
        }

        await using var db = _database.CreateContext();
        var reloaded = await db.Accounts.SingleAsync(a => a.Id == AccountId.Savings);
        Assert.Equal(Money.From(1000m), reloaded.Balance);
        Assert.Equal(0, reloaded.Version);
        Assert.Equal(1, await db.Transactions.CountAsync());
    }

    [Fact]
    public async Task RepeatedLookupsInOneScopeReturnTheSameTrackedInstance()
    {
        await using var scope = _services.CreateAsyncScope();
        var accounts = scope.ServiceProvider.GetRequiredService<IAccountRepository>();

        var first = await accounts.FindAsync(AccountId.Checking, CancellationToken.None);
        var second = await accounts.FindAsync(AccountId.Checking, CancellationToken.None);

        Assert.Same(first, second);
        Assert.Null(await accounts.FindAsync(AccountId.From("brokerage"), CancellationToken.None));
    }

    private async Task<TransactionSummary> RunAsync<THandler, TCommand>(
        Func<THandler, TCommand, Task<TransactionSummary>> invoke, TCommand command)
        where THandler : notnull
    {
        await using var scope = _services.CreateAsyncScope();
        return await invoke(scope.ServiceProvider.GetRequiredService<THandler>(), command);
    }

    private sealed class MutableClock : IClock
    {
        public DateTime UtcNow { get; set; }
    }
}
