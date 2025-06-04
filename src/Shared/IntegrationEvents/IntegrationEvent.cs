namespace Shared.IntegrationEvents;

public abstract class IntegrationEvent
{
    public string Id { get; init; } = Guid.NewGuid().ToString();

    public DateTimeOffset DateOccurred { get; init; } = DateTimeOffset.UtcNow;
}