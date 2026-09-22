using Atm.Application.Accounts;
using Atm.Application.Ports;
using Atm.Domain;

namespace Atm.Application.Transactions;

public sealed class TransferHandler
{
    private readonly IAccountRepository _accounts;
    private readonly ITransactionRepository _transactions;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClock _clock;

    public TransferHandler(
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

    public async Task<TransactionSummary> HandleAsync(TransferCommand command, CancellationToken cancellationToken)
    {
        var amount = Money.From(command.Amount);
        var source = await _accounts.RequireAsync(command.SourceAccountId, cancellationToken);

        // Reuse the loaded instance when both ids match so the domain, not the
        // repository, decides how a same-account transfer is reported.
        var destination = command.DestinationAccountId == command.SourceAccountId
            ? source
            : await _accounts.RequireAsync(command.DestinationAccountId, cancellationToken);

        var transaction = source.TransferTo(destination, amount, _clock.UtcNow);

        await _transactions.AddAsync(transaction, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return TransactionSummary.FromDomain(transaction);
    }
}
