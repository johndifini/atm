namespace Atm.IntegrationTests;

public sealed class ArchitectureSmokeTests
{
    [Fact]
    public void WebAssemblyIsAvailableToIntegrationHarness()
    {
        Assert.Equal("Atm.Web", typeof(Program).Assembly.GetName().Name);
    }
}
