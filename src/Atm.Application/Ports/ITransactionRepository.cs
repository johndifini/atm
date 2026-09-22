using Atm.Domain.Transactions;

namespace Atm.Application.Ports;

/// <summary>
/// Append-only access to transaction history.
/// </summary>
public interface ITransactionRepository
{
    /// <summary>Stage a history record; it is persisted by <see cref="IUnitOfWork.CommitAsync"/>.</summary>
    Task AddAsync(Transaction transaction, CancellationToken cancellationToken);

    /// <summary>All history records, newest first.</summary>
    Task<IReadOnlyList<Transaction>> ListNewestFirstAsync(CancellationToken cancellationToken);
}
