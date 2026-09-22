using Atm.Application.Ports;

namespace Atm.Application.Tests.Fakes;

internal sealed class FixedClock : IClock
{
    public static readonly DateTime Default = new(2026, 9, 21, 15, 30, 0, DateTimeKind.Utc);

    public FixedClock(DateTime? utcNow = null)
    {
        UtcNow = utcNow ?? Default;
    }

    public DateTime UtcNow { get; set; }
}
