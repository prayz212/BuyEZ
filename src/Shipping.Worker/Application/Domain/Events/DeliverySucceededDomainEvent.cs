using Shared.Common;

namespace ShippingWorker.Application.Domain.Events;

public class DeliverySucceededDomainEvent : DomainEvent
{
    public string OrderId { get; }

    public string JobId { get; }

    public DeliverySucceededDomainEvent(string orderId, string jobId)
    {
        OrderId = orderId;
        JobId = jobId;
    }
}