namespace Atm.Application.Errors;

/// <summary>
/// Raised by <see cref="Ports.IUnitOfWork.CommitAsync"/> when an involved account
/// was changed by another operation after it was loaded. Nothing was written; the
/// caller may safely retry with fresh balances.
/// </summary>
public sealed class ConcurrencyConflictException : Exception
{
    public ConcurrencyConflictException()
        : base("The account was updated by another operation. Please review the balances and try again.")
    {
    }

    public ConcurrencyConflictException(Exception innerException)
        : base("The account was updated by another operation. Please review the balances and try again.", innerException)
    {
    }
}
