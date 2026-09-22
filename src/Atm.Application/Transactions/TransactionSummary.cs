using Atm.Domain;
using Atm.Domain.Accounts;
using Atm.Domain.Transactions;

namespace Atm.Application.Transactions;

/// <summary>
/// Immutable read model of one history record; also the receipt returned by
/// every successful command.
/// </summary>
public sealed record TransactionSummary(
    Guid Id,
    DateTime OccurredAtUtc,
    TransactionType Type,
    Money Amount,
    AccountId? SourceAccountId,
    Money? SourceBalanceAfter,
    AccountId? DestinationAccountId,
    Money? DestinationBalanceAfter)
{
    public static TransactionSummary FromDomain(Transaction transaction) => new(
        transaction.Id,
        transaction.OccurredAtUtc,
        transaction.Type,
        transaction.Amount,
        transaction.SourceAccountId,
        transaction.SourceBalanceAfter,
        transaction.DestinationAccountId,
        transaction.DestinationBalanceAfter);
}
