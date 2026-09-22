using Atm.Application.Errors;

namespace Atm.Application.Ports;

/// <summary>
/// Commits every staged change — mutated accounts and appended history — in one
/// atomic database transaction, or none of them.
/// </summary>
public interface IUnitOfWork
{
    /// <exception cref="ConcurrencyConflictException">
    /// Another operation changed an involved account since it was loaded; nothing was written.
    /// </exception>
    Task CommitAsync(CancellationToken cancellationToken);
}
