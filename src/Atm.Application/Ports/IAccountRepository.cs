using Atm.Domain.Accounts;

namespace Atm.Application.Ports;

/// <summary>
/// Read and track accounts. Implementations must return the same tracked
/// instance for repeated lookups within one unit of work so that mutations made
/// through the domain are observed by <see cref="IUnitOfWork.CommitAsync"/>.
/// </summary>
public interface IAccountRepository
{
    Task<Account?> FindAsync(AccountId id, CancellationToken cancellationToken);

    /// <summary>All accounts in stable display order.</summary>
    Task<IReadOnlyList<Account>> ListAsync(CancellationToken cancellationToken);
}
