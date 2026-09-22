using Atm.Application;

namespace Atm.Application.Tests;

public sealed class ArchitectureSmokeTests
{
    [Fact]
    public void ApplicationAssemblyHasStableName()
    {
        Assert.Equal("Atm.Application", typeof(ApplicationAssemblyReference).Assembly.GetName().Name);
    }
}
