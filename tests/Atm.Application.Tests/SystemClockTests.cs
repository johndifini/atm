using Atm.Application.Ports;

namespace Atm.Application.Tests;

public sealed class SystemClockTests
{
    [Fact]
    public void SystemClockReportsUtc()
    {
        var before = DateTime.UtcNow;

        var now = new SystemClock().UtcNow;

        Assert.Equal(DateTimeKind.Utc, now.Kind);
        Assert.InRange(now, before, DateTime.UtcNow.AddSeconds(1));
    }
}
