using System.Text.Json;

namespace Atm.IntegrationTests.Http;

public sealed class LaunchSettingsTests
{
    [Fact]
    public void DevelopmentProfilesUseBindableLocalPorts()
    {
        var repositoryRoot = new DirectoryInfo(AppContext.BaseDirectory);
        while (repositoryRoot is not null && !File.Exists(Path.Combine(repositoryRoot.FullName, "Atm.sln")))
        {
            repositoryRoot = repositoryRoot.Parent;
        }

        Assert.NotNull(repositoryRoot);
        var settingsPath = Path.Combine(repositoryRoot.FullName, "src", "Atm.Web", "Properties", "launchSettings.json");
        using var settings = JsonDocument.Parse(File.ReadAllText(settingsPath));

        foreach (var profile in settings.RootElement.GetProperty("profiles").EnumerateObject())
        {
            var addresses = profile.Value.GetProperty("applicationUrl").GetString()!.Split(';');
            Assert.NotEmpty(addresses);
            foreach (var address in addresses)
            {
                var uri = new Uri(address);
                Assert.True(uri.IsLoopback);
                Assert.InRange(uri.Port, 1, 65535);
            }
        }
    }
}
