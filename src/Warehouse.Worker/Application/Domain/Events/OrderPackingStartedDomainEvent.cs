using Shared.Common;

namespace WarehouseWorker.Application.Domain.Events;

public class OrderPackingStartedDomainEvent : DomainEvent
{
    public string OrderId { get; }

    public string JobId { get; }

    public OrderPackingStartedDomainEvent(string orderId, string jobId)
    {
        OrderId = orderId;
        JobId = jobId;
    }
}