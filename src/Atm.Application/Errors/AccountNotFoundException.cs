using Atm.Domain.Accounts;

namespace Atm.Application.Errors;

public sealed class AccountNotFoundException : Exception
{
    public AccountNotFoundException(AccountId accountId)
        : base($"Account '{accountId}' does not exist.")
    {
        AccountId = accountId;
    }

    public AccountId AccountId { get; }
}
