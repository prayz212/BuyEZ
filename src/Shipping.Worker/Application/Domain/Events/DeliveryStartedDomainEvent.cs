using Shared.Common;

namespace ShippingWorker.Application.Domain.Events;

public class DeliveryStartedDomainEvent : DomainEvent
{
    public string OrderId { get; }

    public string JobId { get; }

    public DeliveryStartedDomainEvent(string orderId, string jobId)
    {
        OrderId = orderId;
        JobId = jobId;
    }
}