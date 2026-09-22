using Atm.Application.Accounts;
using Atm.Application.Errors;
using Atm.Application.Transactions;
using Atm.Domain.Accounts;
using Atm.Domain.Errors;
using Atm.Domain.Transactions;
using Atm.Web.Operations;
using Atm.Web.Presentation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Atm.Web.Pages;

/// <summary>
/// The single dashboard. Each operation posts to its own handler; success
/// redirects back to GET (Post/Redirect/Get) with a one-time confirmation in
/// TempData, so a refresh never repeats an operation. Failures re-render the
/// page with the submitted values and the error mapped to the relevant field.
/// </summary>
public sealed class IndexModel : PageModel
{
    private readonly GetAccountsQuery _getAccounts;
    private readonly GetTransactionHistoryQuery _getHistory;
    private readonly DepositHandler _deposit;
    private readonly WithdrawHandler _withdraw;
    private readonly TransferHandler _transfer;
    private Dictionary<AccountId, string> _names = new();

    public IndexModel(
        GetAccountsQuery getAccounts,
        GetTransactionHistoryQuery getHistory,
        DepositHandler deposit,
        WithdrawHandler withdraw,
        TransferHandler transfer)
    {
        _getAccounts = getAccounts;
        _getHistory = getHistory;
        _deposit = deposit;
        _withdraw = withdraw;
        _transfer = transfer;
    }

    public IReadOnlyList<AccountSummary> Accounts { get; private set; } = [];

    public IReadOnlyList<TransactionSummary> History { get; private set; } = [];

    public Operation ActiveOperation { get; private set; } = Operation.Deposit;

    public DepositInput Deposit { get; private set; } = new() { AccountId = AccountId.Checking.Value };

    public WithdrawInput Withdraw { get; private set; } = new() { AccountId = AccountId.Checking.Value };

    public TransferInput Transfer { get; private set; } = new()
    {
        SourceAccountId = AccountId.Checking.Value,
        DestinationAccountId = AccountId.Savings.Value,
    };

    [TempData]
    public string? SuccessMessage { get; set; }

    public string AccountName(AccountId id) => _names.GetValueOrDefault(id, id.Value);

    /// <summary>"true" when the field has a validation error, otherwise null so Razor omits the attribute.</summary>
    public string? Invalid(string key) =>
        ModelState.TryGetValue(key, out var entry) && entry.Errors.Count > 0 ? "true" : null;

    public async Task OnGetAsync(string? op, CancellationToken cancellationToken)
    {
        ActiveOperation = OperationCatalog.Parse(op);
        await LoadAsync(cancellationToken);
    }

    public Task<IActionResult> OnPostDepositAsync(
        [Bind(Prefix = nameof(Deposit))] DepositInput input, CancellationToken cancellationToken)
    {
        Deposit = input;
        return ExecuteAsync(
            Operation.Deposit,
            () => _deposit.HandleAsync(
                new DepositCommand(AccountId.From(input.AccountId!), input.Amount!.Value), cancellationToken),
            accountField: _ => "Deposit.AccountId",
            amountField: "Deposit.Amount",
            cancellationToken);
    }

    public Task<IActionResult> OnPostWithdrawAsync(
        [Bind(Prefix = nameof(Withdraw))] WithdrawInput input, CancellationToken cancellationToken)
    {
        Withdraw = input;
        return ExecuteAsync(
            Operation.Withdraw,
            () => _withdraw.HandleAsync(
                new WithdrawCommand(AccountId.From(input.AccountId!), input.Amount!.Value), cancellationToken),
            accountField: _ => "Withdraw.AccountId",
            amountField: "Withdraw.Amount",
            cancellationToken);
    }

    public Task<IActionResult> OnPostTransferAsync(
        [Bind(Prefix = nameof(Transfer))] TransferInput input, CancellationToken cancellationToken)
    {
        Transfer = input;
        return ExecuteAsync(
            Operation.Transfer,
            () => _transfer.HandleAsync(
                new TransferCommand(
                    AccountId.From(input.SourceAccountId!),
                    AccountId.From(input.DestinationAccountId!),
                    input.Amount!.Value),
                cancellationToken),
            accountField: id => string.Equals(id.Value, input.DestinationAccountId?.Trim(), StringComparison.OrdinalIgnoreCase)
                ? "Transfer.DestinationAccountId"
                : "Transfer.SourceAccountId",
            amountField: "Transfer.Amount",
            cancellationToken);
    }

    private async Task<IActionResult> ExecuteAsync(
        Operation operation,
        Func<Task<TransactionSummary>> run,
        Func<AccountId, string> accountField,
        string amountField,
        CancellationToken cancellationToken)
    {
        ActiveOperation = operation;

        if (ModelState.IsValid)
        {
            try
            {
                await LoadAccountsAsync(cancellationToken);
                var receipt = await run();
                SuccessMessage = Describe(receipt);
                return RedirectToPage(new { op = operation.Slug() });
            }
            catch (InvalidAmountException exception)
            {
                ModelState.AddModelError(amountField, exception.Message);
            }
            catch (InsufficientFundsException exception)
            {
                ModelState.AddModelError(
                    amountField,
                    $"Insufficient funds: {Format.Currency(exception.Available)} available in {AccountName(exception.AccountId)}.");
            }
            catch (SameAccountTransferException)
            {
                ModelState.AddModelError("Transfer.DestinationAccountId", "Choose a different destination account.");
            }
            catch (AccountNotFoundException exception)
            {
                ModelState.AddModelError(accountField(exception.AccountId), "Choose a valid account.");
            }
            catch (ConcurrencyConflictException exception)
            {
                ModelState.AddModelError(string.Empty, exception.Message);
            }
        }

        await LoadAsync(cancellationToken);
        return Page();
    }

    private async Task LoadAsync(CancellationToken cancellationToken)
    {
        await LoadAccountsAsync(cancellationToken);
        History = await _getHistory.ExecuteAsync(cancellationToken);
    }

    private async Task LoadAccountsAsync(CancellationToken cancellationToken)
    {
        Accounts = await _getAccounts.ExecuteAsync(cancellationToken);
        _names = Accounts.ToDictionary(a => a.Id, a => a.Name);
    }

    private string Describe(TransactionSummary receipt)
    {
        var amount = Format.Currency(receipt.Amount);
        switch (receipt.Type)
        {
            case TransactionType.Deposit:
            {
                var name = AccountName(receipt.DestinationAccountId!.Value);
                return $"Deposited {amount} to {name}. {name} balance is now {Format.Currency(receipt.DestinationBalanceAfter!.Value)}.";
            }

            case TransactionType.Withdrawal:
            {
                var name = AccountName(receipt.SourceAccountId!.Value);
                return $"Withdrew {amount} from {name}. {name} balance is now {Format.Currency(receipt.SourceBalanceAfter!.Value)}.";
            }

            default:
            {
                var source = AccountName(receipt.SourceAccountId!.Value);
                var destination = AccountName(receipt.DestinationAccountId!.Value);
                return $"Transferred {amount} from {source} to {destination}. "
                    + $"{source} balance is now {Format.Currency(receipt.SourceBalanceAfter!.Value)}; "
                    + $"{destination} balance is now {Format.Currency(receipt.DestinationBalanceAfter!.Value)}.";
            }
        }
    }
}
