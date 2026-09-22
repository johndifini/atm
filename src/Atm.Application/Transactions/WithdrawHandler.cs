using Atm.Application.Accounts;
using Atm.Application.Ports;
using Atm.Domain;

namespace Atm.Application.Transactions;

public sealed class WithdrawHandler
{
    private readonly IAccountRepository _accounts;
    private readonly ITransactionRepository _transactions;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClock _clock;

    public WithdrawHandler(
        IAccountRepository accounts,
        ITransactionRepository transactions,
        IUnitOfWork unitOfWork,
        IClock clock)
    {
        _accounts = accounts;
        _transactions = transactions;
        _unitOfWork = unitOfWork;
        _clock = clock;
    }

    public async Task<TransactionSummary> HandleAsync(WithdrawCommand command, CancellationToken cancellationToken)
    {
        var amount = Money.From(command.Amount);
        var account = await _accounts.RequireAsync(command.AccountId, cancellationToken);

        var transaction = account.Withdraw(amount, _clock.UtcNow);

        await _transactions.AddAsync(transaction, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return TransactionSummary.FromDomain(transaction);
    }
}
