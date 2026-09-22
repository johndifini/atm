namespace Atm.Domain.Errors;

/// <summary>
/// Base type for business-rule violations. Messages are safe to show to the user.
/// </summary>
public abstract class DomainException : Exception
{
    protected DomainException(string message)
        : base(message)
    {
    }
}
