using System.Net;
using System.Text.RegularExpressions;

namespace Atm.IntegrationTests.Http;

public sealed class RestartAndAssetsTests
{
    [Fact]
    public async Task BalancesAndHistorySurviveAHostRestart()
    {
        var databasePath = AtmWebApplicationFactory.NewDatabasePath();

        using (var first = new AtmWebApplicationFactory(databasePath, deleteDatabaseOnDispose: false))
        {
            var client = new DashboardClient(first);
            var response = await client.PostAsync("Transfer", new()
            {
                ["Transfer.SourceAccountId"] = "savings",
                ["Transfer.DestinationAccountId"] = "checking",
                ["Transfer.Amount"] = "123.45",
            });
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        }

        using var second = new AtmWebApplicationFactory(databasePath);
        var html = await new DashboardClient(second).GetAsync();

        Assert.Contains("$1,123.45", html);
        Assert.Contains("$876.55", html);
        Assert.Single(Regex.Matches(html, "<td data-label=\"Type\">Transfer</td>"));
        Assert.DoesNotContain("Success:", html);
    }

    [Fact]
    public async Task TheStylesheetAndScriptReferencedByThePageResolve()
    {
        using var factory = new AtmWebApplicationFactory();
        var http = factory.CreateClient();
        var html = await http.GetStringAsync("/");

        var stylesheet = Regex.Match(html, "<link rel=\"stylesheet\" href=\"([^\"]+)\"").Groups[1].Value;
        var script = Regex.Match(html, "<script src=\"([^\"]+)\"").Groups[1].Value;
        Assert.NotEmpty(stylesheet);
        Assert.NotEmpty(script);

        var css = await http.GetAsync(stylesheet);
        var js = await http.GetAsync(script);
        Assert.Equal(HttpStatusCode.OK, css.StatusCode);
        Assert.Equal("text/css", css.Content.Headers.ContentType?.MediaType);
        Assert.Equal(HttpStatusCode.OK, js.StatusCode);
    }
}
