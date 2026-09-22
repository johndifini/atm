using Atm.Domain.Accounts;

namespace Atm.Domain.Errors;

public sealed class SameAccountTransferException : DomainException
{
    public SameAccountTransferException(AccountId accountId)
        : base("A transfer requires two different accounts.")
    {
        AccountId = accountId;
    }

    public AccountId AccountId { get; }
}
