using Atm.Domain.Errors;
using Atm.Domain.Transactions;

namespace Atm.Domain.Accounts;

/// <summary>
/// An account whose balance can never become negative. Every successful mutation
/// returns the <see cref="Transaction"/> that records it, so callers can persist
/// the balance change and its history atomically. A failed operation leaves the
/// account (and, for transfers, the counterparty) untouched.
/// </summary>
public sealed class Account
{
    public Account(AccountId id, string name, Money openingBalance)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Id = id;
        Name = name.Trim();
        Balance = openingBalance;
    }

    public AccountId Id { get; }

    public string Name { get; }

    public Money Balance { get; private set; }

    /// <summary>
    /// Incremented on every successful mutation; persistence uses it as an
    /// optimistic-concurrency token so concurrent updates cannot be lost.
    /// </summary>
    public int Version { get; private set; }

    public Transaction Deposit(Money amount, DateTime occurredAtUtc)
    {
        EnsurePositive(amount);
        EnsureUtc(occurredAtUtc);

        var balanceAfter = Balance + amount;
        var transaction = new Transaction(
            NewTransactionId(),
            occurredAtUtc,
            TransactionType.Deposit,
            amount,
            sourceAccountId: null,
            sourceBalanceAfter: null,
            destinationAccountId: Id,
            destinationBalanceAfter: balanceAfter);

        Apply(balanceAfter);
        return transaction;
    }

    public Transaction Withdraw(Money amount, DateTime occurredAtUtc)
    {
        EnsurePositive(amount);
        EnsureUtc(occurredAtUtc);
        EnsureSufficientFunds(amount);

        var balanceAfter = Balance - amount;
        var transaction = new Transaction(
            NewTransactionId(),
            occurredAtUtc,
            TransactionType.Withdrawal,
            amount,
            sourceAccountId: Id,
            sourceBalanceAfter: balanceAfter,
            destinationAccountId: null,
            destinationBalanceAfter: null);

        Apply(balanceAfter);
        return transaction;
    }

    public Transaction TransferTo(Account destination, Money amount, DateTime occurredAtUtc)
    {
        ArgumentNullException.ThrowIfNull(destination);
        EnsurePositive(amount);
        EnsureUtc(occurredAtUtc);

        if (destination.Id == Id)
        {
            throw new SameAccountTransferException(Id);
        }

        EnsureSufficientFunds(amount);

        var sourceBalanceAfter = Balance - amount;
        var destinationBalanceAfter = destination.Balance + amount;
        var transaction = new Transaction(
            NewTransactionId(),
            occurredAtUtc,
            TransactionType.Transfer,
            amount,
            sourceAccountId: Id,
            sourceBalanceAfter: sourceBalanceAfter,
            destinationAccountId: destination.Id,
            destinationBalanceAfter: destinationBalanceAfter);

        Apply(sourceBalanceAfter);
        destination.Apply(destinationBalanceAfter);
        return transaction;
    }

    private void Apply(Money balanceAfter)
    {
        Balance = balanceAfter;
        Version++;
    }

    private void EnsureSufficientFunds(Money amount)
    {
        if (amount > Balance)
        {
            throw new InsufficientFundsException(Id, Balance, amount);
        }
    }

    private static void EnsurePositive(Money amount)
    {
        if (amount.IsZero)
        {
            throw new InvalidAmountException(amount.Value, "Amount must be greater than zero.");
        }
    }

    private static void EnsureUtc(DateTime occurredAtUtc)
    {
        if (occurredAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("Timestamps must be UTC.", nameof(occurredAtUtc));
        }
    }

    private static Guid NewTransactionId() => Guid.CreateVersion7();
}
