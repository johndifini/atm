using Atm.Domain;
using Atm.Domain.Accounts;
using Microsoft.EntityFrameworkCore;

namespace Atm.Infrastructure.Persistence;

/// <summary>
/// Applies pending migrations and, only when the account table is empty (a
/// freshly created database), seeds Checking and Savings at the opening balance.
/// Balances are never reset by a later run.
/// </summary>
public static class AtmDatabaseInitializer
{
    public static readonly Money OpeningBalance = Money.From(1000m);

    public static async Task InitializeAsync(AtmDbContext db, CancellationToken cancellationToken = default)
    {
        await db.Database.MigrateAsync(cancellationToken);

        if (await db.Accounts.AnyAsync(cancellationToken))
        {
            return;
        }

        db.Accounts.AddRange(
            new Account(AccountId.Checking, "Checking", OpeningBalance),
            new Account(AccountId.Savings, "Savings", OpeningBalance));

        await db.SaveChangesAsync(cancellationToken);
    }
}
