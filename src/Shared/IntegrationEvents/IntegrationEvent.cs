namespace Shared.IntegrationEvents;

public abstract class IntegrationEvent
{
    public string Id { get; }

    public DateTimeOffset DateOccurred { get; }

    protected IntegrationEvent()
    {
        Id = Guid.NewGuid().ToString();
        DateOccurred = DateTimeOffset.UtcNow;
    }
}