using Atm.Domain;
using Atm.Domain.Accounts;

namespace Atm.Application.Accounts;

public sealed record AccountSummary(AccountId Id, string Name, Money Balance)
{
    public static AccountSummary FromDomain(Account account) => new(account.Id, account.Name, account.Balance);
}
