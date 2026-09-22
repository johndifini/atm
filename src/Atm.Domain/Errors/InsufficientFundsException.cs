using Atm.Domain.Accounts;

namespace Atm.Domain.Errors;

public sealed class InsufficientFundsException : DomainException
{
    public InsufficientFundsException(AccountId accountId, Money available, Money requested)
        : base($"Insufficient funds: {available} available, {requested} requested.")
    {
        AccountId = accountId;
        Available = available;
        Requested = requested;
    }

    public AccountId AccountId { get; }

    public Money Available { get; }

    public Money Requested { get; }
}
