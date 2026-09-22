using Atm.Domain;

namespace Atm.Domain.Tests;

public sealed class ArchitectureSmokeTests
{
    [Fact]
    public void DomainAssemblyHasStableName()
    {
        Assert.Equal("Atm.Domain", typeof(DomainAssemblyReference).Assembly.GetName().Name);
    }
}
