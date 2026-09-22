using Atm.Domain;
using Atm.Domain.Accounts;
using Atm.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Atm.IntegrationTests.Persistence;

public sealed class DatabaseInitializerTests : IDisposable
{
    private readonly SqliteDatabase _database = new();

    [Fact]
    public async Task FreshDatabaseSeedsExactlyCheckingAndSavingsAtOpeningBalance()
    {
        await _database.InitializeAsync();

        await using var db = _database.CreateContext();
        var accounts = await db.Accounts.OrderBy(a => a.Name).ToListAsync();

        Assert.Collection(
            accounts,
            a =>
            {
                Assert.Equal(AccountId.Checking, a.Id);
                Assert.Equal("Checking", a.Name);
                Assert.Equal(Money.From(1000m), a.Balance);
                Assert.Equal(0, a.Version);
            },
            a =>
            {
                Assert.Equal(AccountId.Savings, a.Id);
                Assert.Equal("Savings", a.Name);
                Assert.Equal(Money.From(1000m), a.Balance);
                Assert.Equal(0, a.Version);
            });
        Assert.Empty(await db.Transactions.ToListAsync());
    }

    [Fact]
    public async Task ReinitializingDoesNotReseedOrResetBalances()
    {
        await _database.InitializeAsync();
        await using (var db = _database.CreateContext())
        {
            var checking = await db.Accounts.SingleAsync(a => a.Id == AccountId.Checking);
            db.Transactions.Add(checking.Withdraw(Money.From(150m), DateTime.UtcNow));
            await db.SaveChangesAsync();
        }

        await _database.InitializeAsync();

        await using var verify = _database.CreateContext();
        Assert.Equal(2, await verify.Accounts.CountAsync());
        var reloaded = await verify.Accounts.SingleAsync(a => a.Id == AccountId.Checking);
        Assert.Equal(Money.From(850m), reloaded.Balance);
        Assert.Equal(1, reloaded.Version);
        Assert.Equal(1, await verify.Transactions.CountAsync());
    }

    [Fact]
    public async Task AllMigrationsAreAppliedAndTheModelHasNoPendingChanges()
    {
        await _database.InitializeAsync();

        await using var db = _database.CreateContext();
        Assert.Empty(await db.Database.GetPendingMigrationsAsync());
        Assert.False(db.Database.HasPendingModelChanges(), "Add a migration for the current model.");
    }

    public void Dispose() => _database.Dispose();
}
