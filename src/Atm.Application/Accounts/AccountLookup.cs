using Atm.Application.Errors;
using Atm.Application.Ports;
using Atm.Domain.Accounts;

namespace Atm.Application.Accounts;

internal static class AccountLookup
{
    public static async Task<Account> RequireAsync(
        this IAccountRepository accounts, AccountId id, CancellationToken cancellationToken)
    {
        return await accounts.FindAsync(id, cancellationToken) ?? throw new AccountNotFoundException(id);
    }
}
