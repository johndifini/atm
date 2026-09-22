using Atm.Application.Ports;
using Atm.Domain.Transactions;
using Microsoft.EntityFrameworkCore;

namespace Atm.Infrastructure.Persistence;

internal sealed class TransactionRepository : ITransactionRepository
{
    private readonly AtmDbContext _db;

    public TransactionRepository(AtmDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(Transaction transaction, CancellationToken cancellationToken)
    {
        await _db.Transactions.AddAsync(transaction, cancellationToken);
    }

    public async Task<IReadOnlyList<Transaction>> ListNewestFirstAsync(CancellationToken cancellationToken)
    {
        return await _db.Transactions
            .AsNoTracking()
            .OrderByDescending(t => t.OccurredAtUtc)
            .ThenByDescending(t => t.Id)
            .ToListAsync(cancellationToken);
    }
}
