using Atm.Application.Ports;

namespace Atm.Application.Transactions;

public sealed class GetTransactionHistoryQuery
{
    private readonly ITransactionRepository _transactions;

    public GetTransactionHistoryQuery(ITransactionRepository transactions)
    {
        _transactions = transactions;
    }

    /// <summary>History records, newest first.</summary>
    public async Task<IReadOnlyList<TransactionSummary>> ExecuteAsync(CancellationToken cancellationToken)
    {
        var transactions = await _transactions.ListNewestFirstAsync(cancellationToken);
        return transactions.Select(TransactionSummary.FromDomain).ToList();
    }
}
