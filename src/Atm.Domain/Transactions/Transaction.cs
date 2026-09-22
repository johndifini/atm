using Atm.Domain.Accounts;

namespace Atm.Domain.Transactions;

/// <summary>
/// An immutable history record of one completed operation. Each affected side
/// carries the account it touched and that account's balance immediately after
/// the operation, so the ledger can be explained without re-reading account rows.
/// </summary>
/// <remarks>
/// Deposits populate only the destination side, withdrawals only the source side,
/// and transfers populate both with two distinct accounts.
/// </remarks>
public sealed class Transaction
{
    public Transaction(
        Guid id,
        DateTime occurredAtUtc,
        TransactionType type,
        Money amount,
        AccountId? sourceAccountId,
        Money? sourceBalanceAfter,
        AccountId? destinationAccountId,
        Money? destinationBalanceAfter)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Transaction identifier cannot be empty.", nameof(id));
        }

        if (occurredAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("Transaction timestamp must be UTC.", nameof(occurredAtUtc));
        }

        if (amount.IsZero)
        {
            throw new ArgumentException("Transaction amount must be greater than zero.", nameof(amount));
        }

        if (sourceAccountId.HasValue != sourceBalanceAfter.HasValue)
        {
            throw new ArgumentException(
                "Source account and source balance must be supplied together.", nameof(sourceBalanceAfter));
        }

        if (destinationAccountId.HasValue != destinationBalanceAfter.HasValue)
        {
            throw new ArgumentException(
                "Destination account and destination balance must be supplied together.",
                nameof(destinationBalanceAfter));
        }

        switch (type)
        {
            case TransactionType.Deposit:
                Require(!sourceAccountId.HasValue, "A deposit has no source account.");
                Require(destinationAccountId.HasValue, "A deposit requires a destination account.");
                break;
            case TransactionType.Withdrawal:
                Require(sourceAccountId.HasValue, "A withdrawal requires a source account.");
                Require(!destinationAccountId.HasValue, "A withdrawal has no destination account.");
                break;
            case TransactionType.Transfer:
                Require(sourceAccountId.HasValue, "A transfer requires a source account.");
                Require(destinationAccountId.HasValue, "A transfer requires a destination account.");
                Require(sourceAccountId != destinationAccountId, "A transfer requires two different accounts.");
                break;
            default:
                throw new ArgumentException($"Unknown transaction type '{type}'.", nameof(type));
        }

        Id = id;
        OccurredAtUtc = occurredAtUtc;
        Type = type;
        Amount = amount;
        SourceAccountId = sourceAccountId;
        SourceBalanceAfter = sourceBalanceAfter;
        DestinationAccountId = destinationAccountId;
        DestinationBalanceAfter = destinationBalanceAfter;
    }

    public Guid Id { get; }

    public DateTime OccurredAtUtc { get; }

    public TransactionType Type { get; }

    public Money Amount { get; }

    public AccountId? SourceAccountId { get; }

    public Money? SourceBalanceAfter { get; }

    public AccountId? DestinationAccountId { get; }

    public Money? DestinationBalanceAfter { get; }

    private static void Require(bool condition, string message)
    {
        if (!condition)
        {
            throw new ArgumentException(message);
        }
    }
}
