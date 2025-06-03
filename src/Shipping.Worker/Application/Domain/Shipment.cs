using Shared.Common;
using Shared.Common.Interfaces;

namespace ShippingWorker.Application.Domain;

public class Shipment : AuditableEntity, IAggregateRoot
{
    public string Id { get; init; } = string.Empty;

    public ShipmentStatus Status { get; private set; } = ShipmentStatus.FindingDriver;

    public string Reason { get; private set; } = string.Empty;

    public string OrderId { get; private set; } = string.Empty;

    // Navigation Properties
    private List<ShipmentTrackingEvent> _trackingEvents = [];
    public IReadOnlyList<ShipmentTrackingEvent> TrackingEvents => _trackingEvents.AsReadOnly();

    // Domain Events property
    private readonly List<DomainEvent> _domainEvents = [];
    public IReadOnlyList<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    // Constructors
    /*
        - By default, compiler will create a default parameterless constructor.
            => Compiler will NOT generate a parameterless constructor in case we already defined parameters constructor
     
        - Parameterless constructor for EF Core to instantiate the object with the data got from database
     */
    internal Shipment() { }

    private Shipment(string orderId, string reason)
    {
        Id = Guid.NewGuid().ToString();
        OrderId = orderId;
        Status = ShipmentStatus.FindingDriver;
        Reason = reason;
    }

    public static Shipment CreateNew(string orderId, string? reason = default)
    {
        return new(orderId, reason ?? "New order created.");
    }

    public void AssignDriver(string driverName, string executionHistoryId)
    {
        if (Status != ShipmentStatus.FindingDriver)
            throw new InvalidOperationException($"Cannot change the current status ({Status}) to DriverAssigned.");

        var newStatus = ShipmentStatus.DriverAssigned;
        AddTrackingEvent(executionHistoryId, newStatus);

        Status = newStatus;
        Reason = $"Shipment assigned to driver: {driverName}.";
    }

    public void PickUpOrder(string executionHistoryId)
    {
        if (Status != ShipmentStatus.DriverAssigned)
            throw new InvalidOperationException($"Cannot change the current status ({Status}) to PickingUpOrder.");

        var newStatus = ShipmentStatus.PickingUpOrder;
        AddTrackingEvent(executionHistoryId, newStatus);

        Status = newStatus;
        Reason = "Driver is picking up the order.";
    }

    public void DeliverOrder(string executionHistoryId)
    {
        if (Status != ShipmentStatus.PickingUpOrder)
            throw new InvalidOperationException($"Cannot change the current status ({Status}) to DeliveringOrder.");

        var newStatus = ShipmentStatus.DeliveringOrder;
        AddTrackingEvent(executionHistoryId, newStatus);

        Status = newStatus;
        Reason = "Driver is delivering the order.";
    }

    private void AddTrackingEvent(string executionHistoryId, ShipmentStatus newStatus)
    {
        _trackingEvents.Add(
            ShipmentTrackingEvent.CreateNew(
                this,
                executionHistoryId,
                newStatus));
    }

    public void AddDomainEvent(DomainEvent @event)
    {
        _domainEvents.Add(@event);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
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