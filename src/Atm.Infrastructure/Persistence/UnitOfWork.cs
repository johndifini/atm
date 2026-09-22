using Atm.Application.Errors;
using Atm.Application.Ports;
using Microsoft.EntityFrameworkCore;

namespace Atm.Infrastructure.Persistence;

/// <summary>
/// One <see cref="DbContext.SaveChangesAsync(CancellationToken)"/> call writes every
/// tracked change inside a single database transaction, so a transfer's two
/// account updates and its history row commit together or not at all.
/// </summary>
internal sealed class UnitOfWork : IUnitOfWork
{
    private readonly AtmDbContext _db;

    public UnitOfWork(AtmDbContext db)
    {
        _db = db;
    }

    public async Task CommitAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException exception)
        {
            throw new ConcurrencyConflictException(exception);
        }
    }
}
