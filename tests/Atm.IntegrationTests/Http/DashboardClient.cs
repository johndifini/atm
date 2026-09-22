using System.Net;
using System.Text.RegularExpressions;

namespace Atm.IntegrationTests.Http;

/// <summary>
/// Drives the dashboard the way a browser would: cookies are kept, redirects
/// are not followed automatically, and the antiforgery token is read from the
/// rendered form before each POST.
/// </summary>
internal sealed partial class DashboardClient
{
    private readonly HttpClient _http;

    public DashboardClient(AtmWebApplicationFactory factory)
    {
        _http = factory.CreateClient(new() { AllowAutoRedirect = false, HandleCookies = true });
    }

    public async Task<string> GetAsync(string path = "/")
    {
        var response = await _http.GetAsync(path);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());
    }

    public async Task<HttpResponseMessage> PostAsync(string handler, Dictionary<string, string> fields, bool includeToken = true)
    {
        if (includeToken)
        {
            var page = await _http.GetStringAsync("/");
            var match = TokenPattern().Match(page);
            Assert.True(match.Success, "Antiforgery token not found in the rendered page.");
            fields["__RequestVerificationToken"] = match.Groups[1].Value;
        }

        return await _http.PostAsync($"/?handler={handler}", new FormUrlEncodedContent(fields));
    }

    public static async Task<string> BodyAsync(HttpResponseMessage response) =>
        WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());

    [GeneratedRegex("__RequestVerificationToken[^>]*value=\"([^\"]+)\"")]
    private static partial Regex TokenPattern();
}
