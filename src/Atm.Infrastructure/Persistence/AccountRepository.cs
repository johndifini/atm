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
        return await _db.Accounts
            .OrderBy(a => a.Name)
            .ToListAsync(cancellationToken);
    }
}
