using Shared.Common;

namespace OrderAPI.Application.Domain.Events;

public class OrderPlacedDomainEvent : DomainEvent
{
    public string OrderId { get; }

    public OrderPlacedDomainEvent(string id)
    {
        OrderId = id;
    }
}