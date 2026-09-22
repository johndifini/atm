using System.Net;
using System.Text.RegularExpressions;

namespace Atm.IntegrationTests.Http;

public sealed class DashboardTests : IDisposable
{
    private readonly AtmWebApplicationFactory _factory = new();
    private readonly DashboardClient _client;

    public DashboardTests()
    {
        _client = new DashboardClient(_factory);
    }

    public void Dispose() => _factory.Dispose();

    [Fact]
    public async Task DashboardShowsBothAccountsAtOpeningBalanceAndNoHistory()
    {
        var html = await _client.GetAsync();

        Assert.Contains("Checking", html);
        Assert.Contains("Savings", html);
        Assert.Equal(2, Regex.Matches(html, Regex.Escape("$1,000.00")).Count);
        Assert.Contains("No transactions yet.", html);
        Assert.Contains("id=\"tab-deposit\" class=\"tab\" href=\"/?op=deposit\" aria-selected=\"true\"", html);
    }

    [Fact]
    public async Task OperationQueryParameterSelectsTheTab()
    {
        var html = await _client.GetAsync("/?op=transfer");

        Assert.Contains("href=\"/?op=transfer\" aria-selected=\"true\"", html);
        Assert.Contains("href=\"/?op=deposit\" aria-selected=\"false\"", html);
    }

    [Fact]
    public async Task DepositRedirectsThenShowsConfirmationOnce()
    {
        var response = await _client.PostAsync("Deposit", new()
        {
            ["Deposit.AccountId"] = "checking",
            ["Deposit.Amount"] = "250.25",
        });

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/?op=deposit", response.Headers.Location?.ToString());

        var afterRedirect = await _client.GetAsync("/?op=deposit");
        Assert.Contains("Success:", afterRedirect);
        Assert.Contains("Deposited $250.25 to Checking. Checking balance is now $1,250.25.", afterRedirect);
        Assert.Contains("$1,250.25", afterRedirect);
        Assert.Contains("+$250.25", afterRedirect);
        Assert.Single(Regex.Matches(afterRedirect, "<td data-label=\"Type\">Deposit</td>"));

        // A browser refresh re-issues the GET, not the POST: the message is gone
        // and the ledger still holds exactly one deposit.
        var refreshed = await _client.GetAsync("/?op=deposit");
        Assert.DoesNotContain("Success:", refreshed);
        Assert.Single(Regex.Matches(refreshed, "<td data-label=\"Type\">Deposit</td>"));
        Assert.Contains("$1,250.25", refreshed);
    }

    [Fact]
    public async Task WithdrawAndTransferShowTheirReceipts()
    {
        var withdraw = await _client.PostAsync("Withdraw", new()
        {
            ["Withdraw.AccountId"] = "savings",
            ["Withdraw.Amount"] = "400.40",
        });
        Assert.Equal(HttpStatusCode.Redirect, withdraw.StatusCode);
        Assert.Equal("/?op=withdraw", withdraw.Headers.Location?.ToString());
        var afterWithdraw = await _client.GetAsync("/?op=withdraw");
        Assert.Contains("Withdrew $400.40 from Savings. Savings balance is now $599.60.", afterWithdraw);

        var transfer = await _client.PostAsync("Transfer", new()
        {
            ["Transfer.SourceAccountId"] = "checking",
            ["Transfer.DestinationAccountId"] = "savings",
            ["Transfer.Amount"] = "300.75",
        });
        Assert.Equal(HttpStatusCode.Redirect, transfer.StatusCode);
        Assert.Equal("/?op=transfer", transfer.Headers.Location?.ToString());
        var afterTransfer = await _client.GetAsync("/?op=transfer");
        Assert.Contains(
            "Transferred $300.75 from Checking to Savings. Checking balance is now $699.25; Savings balance is now $900.35.",
            afterTransfer);
        Assert.Contains("Checking → Savings", afterTransfer);
        Assert.Contains("Checking $699.25 · Savings $900.35", afterTransfer);

        // Newest first: the transfer row precedes the withdrawal row.
        var transferIndex = afterTransfer.IndexOf("<td data-label=\"Type\">Transfer</td>", StringComparison.Ordinal);
        var withdrawalIndex = afterTransfer.IndexOf("<td data-label=\"Type\">Withdrawal</td>", StringComparison.Ordinal);
        Assert.True(transferIndex > 0 && withdrawalIndex > transferIndex);
    }

    [Fact]
    public async Task OverdraftIsReportedInlineAndChangesNothing()
    {
        var response = await _client.PostAsync("Withdraw", new()
        {
            ["Withdraw.AccountId"] = "checking",
            ["Withdraw.Amount"] = "1000.01",
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var html = await DashboardClient.BodyAsync(response);
        Assert.Contains("Insufficient funds: $1,000.00 available in Checking.", html);
        Assert.Contains("href=\"/?op=withdraw\" aria-selected=\"true\"", html);
        Assert.Contains("value=\"1000.01\"", html);

        var dashboard = await _client.GetAsync();
        Assert.Equal(2, Regex.Matches(dashboard, Regex.Escape("$1,000.00")).Count);
        Assert.Contains("No transactions yet.", dashboard);
    }

    [Fact]
    public async Task SameAccountTransferIsReportedOnTheDestinationField()
    {
        var response = await _client.PostAsync("Transfer", new()
        {
            ["Transfer.SourceAccountId"] = "checking",
            ["Transfer.DestinationAccountId"] = "checking",
            ["Transfer.Amount"] = "10",
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var html = await DashboardClient.BodyAsync(response);
        Assert.Contains("Choose a different destination account.", html);
        Assert.Contains("id=\"transfer-destination-error\" class=\"field-error field-validation-error\"", html);
        Assert.Contains("No transactions yet.", await _client.GetAsync());
    }

    [Theory]
    [InlineData("0", "Enter an amount of at least $0.01.")]
    [InlineData("-5", "Enter an amount of at least $0.01.")]
    [InlineData("", "Enter an amount.")]
    [InlineData("1.005", "Amount cannot have more than two fractional digits.")]
    public async Task InvalidAmountsAreReportedInline(string amount, string expectedMessage)
    {
        var response = await _client.PostAsync("Deposit", new()
        {
            ["Deposit.AccountId"] = "checking",
            ["Deposit.Amount"] = amount,
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var html = await DashboardClient.BodyAsync(response);
        Assert.Contains(expectedMessage, html);
        Assert.Contains("No transactions yet.", await _client.GetAsync());
    }

    [Fact]
    public async Task UnknownAccountIsReportedInline()
    {
        var response = await _client.PostAsync("Deposit", new()
        {
            ["Deposit.AccountId"] = "brokerage",
            ["Deposit.Amount"] = "10",
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Choose a valid account.", await DashboardClient.BodyAsync(response));
    }

    [Fact]
    public async Task PostWithoutAntiforgeryTokenIsRejected()
    {
        var response = await _client.PostAsync("Deposit", new()
        {
            ["Deposit.AccountId"] = "checking",
            ["Deposit.Amount"] = "10",
        }, includeToken: false);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("No transactions yet.", await _client.GetAsync());
    }
}
