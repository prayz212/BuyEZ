using Shared.Common;

namespace ShippingWorker.Application.Domain.Events;

public class DeliveryStartedDomainEvent : DomainEvent
{
    public string OrderId { get; }

    public string DriverName { get; }

    public string JobId { get; }

    public DeliveryStartedDomainEvent(string orderId, string driverName, string jobId)
    {
        OrderId = orderId;
        DriverName = driverName;
        JobId = jobId;
    }
}