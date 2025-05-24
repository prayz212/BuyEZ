namespace Shared.Common.Interfaces;

public interface IAggregateRoot
{
    public IReadOnlyList<DomainEvent> DomainEvents { get; }

    void AddDomainEvent(DomainEvent @event);
    void ClearDomainEvents();
}