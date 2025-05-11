using Shared.Common;

namespace ShippingWorker.Application.Domain;

public class Shipment : AuditableEntity, IHasDomainEvent
{
    public string Id { get; private set; } = string.Empty;

    public ShipmentStatus Status { get; private set; } = ShipmentStatus.FindingDriver;

    public string Reason { get; private set; } = string.Empty;

    public string OrderId { get; private set; } = string.Empty;

    public List<DomainEvent> DomainEvents { get; } = [];

    // Navigation Properties
    private List<ShipmentTrackingEvent> _trackingEvents = [];
    public IReadOnlyList<ShipmentTrackingEvent> TrackingEvents => _trackingEvents.AsReadOnly();

    // Constructors
    /*
        - By default, compiler will create a default parameterless constructor.
            => Compiler will NOT generate a parameterless constructor in case we already defined parameters constructor
     
        - Parameterless constructor for EF Core to instantiate the object with the data got from database
     */
    internal Shipment() { }

    public Shipment(string orderId, string? reason = null)
    {
        Id = Guid.NewGuid().ToString();
        OrderId = orderId;
        Reason = reason ?? "New order created";
    }

    public void UpdateStatus(ShipmentStatus status)
    {
        Status = status;
    }
}

public enum ShipmentStatus
{
    FindingDriver = 1,
    DriverAssigned,
    PickingUpOrder,
    DeliveringOrder,
    DeliverySuccess,
    DeliveryFailed,
    ReturningOrder,
    ReturnedOrder
}