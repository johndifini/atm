namespace Atm.Domain.Errors;

public sealed class InvalidAmountException : DomainException
{
    public InvalidAmountException(decimal attempted, string message)
        : base(message)
    {
        Attempted = attempted;
    }

    public decimal Attempted { get; }
}
