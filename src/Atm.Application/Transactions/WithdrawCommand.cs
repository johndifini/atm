using Atm.Domain.Accounts;

namespace Atm.Application.Transactions;

/// <param name="Amount">Raw user amount; validated as <see cref="Domain.Money"/> by the handler.</param>
public sealed record WithdrawCommand(AccountId AccountId, decimal Amount);
