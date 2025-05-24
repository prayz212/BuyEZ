namespace Shared.Common;

public abstract class DomainEvent
{
    public bool IsPublished { get; private set; }

    public DateTimeOffset DateOccurred { get; protected set; } = DateTimeOffset.UtcNow;

    protected DomainEvent()
    {
        DateOccurred = DateTimeOffset.UtcNow;
    }

    public void MarkAsPublished()
    {
        IsPublished = true;
    }
}