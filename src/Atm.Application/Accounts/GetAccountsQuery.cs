using Atm.Application.Ports;

namespace Atm.Application.Accounts;

public sealed class GetAccountsQuery
{
    private readonly IAccountRepository _accounts;

    public GetAccountsQuery(IAccountRepository accounts)
    {
        _accounts = accounts;
    }

    public async Task<IReadOnlyList<AccountSummary>> ExecuteAsync(CancellationToken cancellationToken)
    {
        var accounts = await _accounts.ListAsync(cancellationToken);
        return accounts.Select(AccountSummary.FromDomain).ToList();
    }
}
