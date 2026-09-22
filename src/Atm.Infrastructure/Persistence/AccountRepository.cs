using Atm.Application.Ports;
using Atm.Domain.Accounts;
using Microsoft.EntityFrameworkCore;

namespace Atm.Infrastructure.Persistence;

internal sealed class AccountRepository : IAccountRepository
{
    private readonly AtmDbContext _db;

    public AccountRepository(AtmDbContext db)
    {
        _db = db;
    }

    public async Task<Account?> FindAsync(AccountId id, CancellationToken cancellationToken)
    {
        return await _db.Accounts.FindAsync([id], cancellationToken);
    }

    public async Task<IReadOnlyList<Account>> ListAsync(CancellationToken cancellationToken)
    {
        // Display reads bypass the identity map so that a request whose commit
        // failed re-renders the balances actually in the database.
        return await _db.Accounts
            .AsNoTracking()
            .OrderBy(a => a.Name)
            .ToListAsync(cancellationToken);
    }
}
