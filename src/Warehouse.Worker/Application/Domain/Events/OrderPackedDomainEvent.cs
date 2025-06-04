using Shared.Common;

namespace WarehouseWorker.Application.Domain.Events;

public class OrderPackedDomainEvent : DomainEvent
{
    public string OrderId { get; }

    public OrderPackedDomainEvent(string id)
    {
        OrderId = id;
    }
}