namespace Atm.Application.Ports;

public interface IClock
{
    /// <summary>The current instant with <see cref="DateTimeKind.Utc"/>.</summary>
    DateTime UtcNow { get; }
}
