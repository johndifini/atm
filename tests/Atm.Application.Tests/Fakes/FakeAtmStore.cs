using Atm.Application.Ports;
using Atm.Domain;
using Atm.Domain.Accounts;
using Atm.Domain.Transactions;

namespace Atm.Application.Tests.Fakes;

/// <summary>
/// In-memory stand-in for the persistence ports. Like an EF Core identity map it
/// hands back the same <see cref="Account"/> instance for repeated lookups, and it
/// records the order of writes so tests can assert that history is appended
/// before the unit of work commits.
/// </summary>
internal sealed class FakeAtmStore : IAccountRepository, ITransactionRepository, IUnitOfWork
{
    private readonly Dictionary<AccountId, Account> _accounts = new();
    private readonly List<Transaction> _transactions = new();

    public List<string> Log { get; } = new();

    public int CommitCount { get; private set; }

    public Exception? CommitFailure { get; set; }

    public IReadOnlyList<Transaction> Transactions => _transactions;

    public Account Seed(AccountId id, string name, decimal balance)
    {
        var account = new Account(id, name, Money.From(balance));
        _accounts[id] = account;
        return account;
    }

    public Task<Account?> FindAsync(AccountId id, CancellationToken cancellationToken)
    {
        Log.Add($"find:{id}");
        return Task.FromResult(_accounts.GetValueOrDefault(id));
    }

    public Task<IReadOnlyList<Account>> ListAsync(CancellationToken cancellationToken)
    {
        Log.Add("list-accounts");
        IReadOnlyList<Account> result = _accounts.Values.ToList();
        return Task.FromResult(result);
    }

    public Task AddAsync(Transaction transaction, CancellationToken cancellationToken)
    {
        Log.Add($"add:{transaction.Type}");
        _transactions.Add(transaction);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<Transaction>> ListNewestFirstAsync(CancellationToken cancellationToken)
    {
        Log.Add("list-transactions");
        IReadOnlyList<Transaction> result = _transactions
            .OrderByDescending(t => t.OccurredAtUtc)
            .ThenByDescending(t => t.Id)
            .ToList();
        return Task.FromResult(result);
    }

    public Task CommitAsync(CancellationToken cancellationToken)
    {
        Log.Add("commit");
        if (CommitFailure is not null)
        {
            return Task.FromException(CommitFailure);
        }

        CommitCount++;
        return Task.CompletedTask;
    }
}
