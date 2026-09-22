using System.Text.RegularExpressions;

namespace Atm.IntegrationTests.Http;

/// <summary>
/// Structural checks that back the manual review in docs/accessibility-review.md.
/// </summary>
public sealed class AccessibilityStructureTests : IDisposable
{
    private readonly AtmWebApplicationFactory _factory = new();
    private readonly DashboardClient _client;

    public AccessibilityStructureTests()
    {
        _client = new DashboardClient(_factory);
    }

    public void Dispose() => _factory.Dispose();

    [Fact]
    public async Task PageHasLanguageLandmarksAndExactlyOneHeadingLevelOne()
    {
        var html = await _client.GetAsync();

        Assert.Contains("<html lang=\"en\">", html);
        Assert.Contains("<main id=\"main\"", html);
        Assert.Contains("class=\"skip-link\" href=\"#main\"", html);
        Assert.Single(Regex.Matches(html, "<h1[\\s>]"));
    }

    [Fact]
    public async Task EveryFormControlHasAnAssociatedLabel()
    {
        var html = await _client.GetAsync();

        var controlIds = Regex.Matches(html, "<(?:input|select)(?![^>]*type=\"hidden\")[^>]*\\sid=\"([^\"]+)\"")
            .Select(m => m.Groups[1].Value)
            .ToList();
        var labelledIds = Regex.Matches(html, "<label for=\"([^\"]+)\"")
            .Select(m => m.Groups[1].Value)
            .ToHashSet();

        Assert.NotEmpty(controlIds);
        Assert.All(controlIds, id => Assert.Contains(id, labelledIds));
    }

    [Fact]
    public async Task TabsAndPanelsAreWiredTogetherAndStayFocusableWithoutScript()
    {
        var html = await _client.GetAsync();

        var ids = Regex.Matches(html, "\\sid=\"([^\"]+)\"").Select(m => m.Groups[1].Value).ToHashSet();
        var tabs = Regex.Matches(html, "<a role=\"tab\"[^>]*>").Select(m => m.Value).ToList();
        var panels = Regex.Matches(html, "<div id=\"[^\"]+\" role=\"tabpanel\"[^>]*>").Select(m => m.Value).ToList();

        Assert.Equal(3, tabs.Count);
        Assert.Equal(3, panels.Count);
        Assert.All(tabs, tab =>
        {
            Assert.DoesNotContain("tabindex", tab);
            Assert.Contains(Regex.Match(tab, "aria-controls=\"([^\"]+)\"").Groups[1].Value, ids);
        });
        Assert.All(panels, panel =>
            Assert.Contains(Regex.Match(panel, "aria-labelledby=\"([^\"]+)\"").Groups[1].Value, ids));
        Assert.Single(tabs, t => t.Contains("aria-selected=\"true\""));
        Assert.Equal(2, panels.Count(p => p.Contains("hidden=\"hidden\"")));
    }

    [Fact]
    public async Task InvalidFieldsAreMarkedWithAriaInvalidAndDescribedByTheirError()
    {
        var response = await _client.PostAsync("Withdraw", new()
        {
            ["Withdraw.AccountId"] = "checking",
            ["Withdraw.Amount"] = "1000.01",
        });
        var html = await DashboardClient.BodyAsync(response);

        var amount = Regex.Match(html, "<input[^>]*name=\"Withdraw.Amount\"[^>]*>").Value;
        Assert.Contains("aria-invalid=\"true\"", amount);
        Assert.Contains("aria-describedby=\"withdraw-amount-error\"", amount);
        Assert.Contains("id=\"withdraw-amount-error\" class=\"field-error field-validation-error\"", html);

        var account = Regex.Match(html, "<select[^>]*name=\"Withdraw.AccountId\"[^>]*>").Value;
        Assert.DoesNotContain("aria-invalid", account);
    }

    [Fact]
    public async Task OutcomesCarryTextLabelsAndLiveRegions()
    {
        var ok = await _client.PostAsync("Deposit", new()
        {
            ["Deposit.AccountId"] = "checking",
            ["Deposit.Amount"] = "1",
        });
        Assert.Equal(System.Net.HttpStatusCode.Redirect, ok.StatusCode);
        var success = await _client.GetAsync("/?op=deposit");
        Assert.Contains("role=\"status\"", success);
        Assert.Contains("<span class=\"notice-label\">Success:</span>", success);

        var failed = await _client.PostAsync("Deposit", new()
        {
            ["Deposit.AccountId"] = "checking",
            ["Deposit.Amount"] = "0",
        });
        var html = await DashboardClient.BodyAsync(failed);

        // Field-level errors are identified on the control itself; the
        // form-level alert region is reserved for model-level errors
        // (see FailureHandlingTests for the concurrency conflict).
        var amount = Regex.Match(html, "<input[^>]*name=\"Deposit.Amount\"[^>]*>").Value;
        Assert.Contains("aria-invalid=\"true\"", amount);
        Assert.Contains("id=\"deposit-amount-error\" class=\"field-error field-validation-error\"", html);
        Assert.Contains("Enter an amount of at least $0.01.", html);
        Assert.DoesNotContain("role=\"alert\"", html);
    }
}
