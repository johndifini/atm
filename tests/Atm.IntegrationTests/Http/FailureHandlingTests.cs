using System.Net;
using Atm.Application.Errors;
using Atm.Application.Ports;
using Microsoft.Extensions.DependencyInjection;

namespace Atm.IntegrationTests.Http;

public sealed class FailureHandlingTests
{
    [Fact]
    public async Task ConcurrencyConflictIsShownAsARetryableFormError()
    {
        using var factory = new AtmWebApplicationFactory(
            configureServices: services => services.AddScoped<IUnitOfWork, ConflictingUnitOfWork>());
        var client = new DashboardClient(factory);

        var response = await client.PostAsync("Withdraw", new()
        {
            ["Withdraw.AccountId"] = "checking",
            ["Withdraw.Amount"] = "10",
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var html = await DashboardClient.BodyAsync(response);
        Assert.Contains("Error:", html);
        Assert.Contains("The account was updated by another operation. Please review the balances and try again.", html);
        Assert.Contains("href=\"/?op=withdraw\" aria-selected=\"true\"", html);
        Assert.Contains("value=\"10\"", html);
        Assert.Contains("No transactions yet.", await client.GetAsync());
    }

    [Fact]
    public async Task UnexpectedFailureShowsTheGenericErrorPageWithoutDetails()
    {
        using var factory = new AtmWebApplicationFactory(
            environment: "Production",
            configureServices: services => services.AddScoped<IUnitOfWork, ExplodingUnitOfWork>());
        var client = new DashboardClient(factory);

        var response = await client.PostAsync("Deposit", new()
        {
            ["Deposit.AccountId"] = "checking",
            ["Deposit.Amount"] = "10",
        });

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        var html = await DashboardClient.BodyAsync(response);
        Assert.Contains("Something went wrong", html);
        Assert.Contains("Back to dashboard", html);
        Assert.DoesNotContain(ExplodingUnitOfWork.SecretDetail, html);
        Assert.DoesNotContain("ExplodingUnitOfWork", html);
        Assert.DoesNotContain("   at ", html);
    }

    [Fact]
    public async Task UnknownOperationFallsBackToDeposit()
    {
        using var factory = new AtmWebApplicationFactory();
        var client = new DashboardClient(factory);

        var html = await client.GetAsync("/?op=nonsense");

        Assert.Contains("href=\"/?op=deposit\" aria-selected=\"true\"", html);
    }

    private sealed class ConflictingUnitOfWork : IUnitOfWork
    {
        public Task CommitAsync(CancellationToken cancellationToken) =>
            throw new ConcurrencyConflictException();
    }

    private sealed class ExplodingUnitOfWork : IUnitOfWork
    {
        public const string SecretDetail = "disk quota exceeded on /var/atm/atm.db";

        public Task CommitAsync(CancellationToken cancellationToken) =>
            throw new InvalidOperationException(SecretDetail);
    }
}
