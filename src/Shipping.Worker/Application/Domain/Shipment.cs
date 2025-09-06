using ShippingWorker.Application.Domain.Events;

using Shared.Common;
using Shared.Common.Interfaces;

namespace ShippingWorker.Application.Domain;

public class Shipment : AuditableEntity, IAggregateRoot
{
    public string Id { get; init; } = string.Empty;

    public ShipmentStatus Status { get; private set; } = ShipmentStatus.FindingDriver;

    public string Reason { get; private set; } = string.Empty;

    public string OrderId { get; private set; } = string.Empty;

    public string? DriverName { get; private set; }

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
        return new(orderId, reason ?? "New order packed.");
    }

    public void AssignDriver(string driverName, string executionHistoryId)
    {
        if (Status != ShipmentStatus.FindingDriver)
            throw new InvalidOperationException($"Cannot change the current status ({Status}) to DriverAssigned.");

        var newStatus = ShipmentStatus.DriverAssigned;
        AddTrackingEvent(executionHistoryId, newStatus);

        Status = newStatus;
        DriverName = driverName;
        Reason = $"Founded shipment driver.";
    }

    public void PickUpOrder(string executionHistoryId)
    {
        if (!IsAllowedToPickUpOrder(Status, DriverName))
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

        _domainEvents.Add(new DeliveryStartedDomainEvent(OrderId, DriverName!, executionHistoryId));
    }

    public void MarkShipmentAsDeliverySuccess(string executionHistoryId)
    {
        if (Status != ShipmentStatus.DeliveringOrder)
            throw new InvalidOperationException($"Cannot change the current status ({Status}) to DeliverySuccess.");

        var newStatus = ShipmentStatus.DeliverySuccess;
        AddTrackingEvent(executionHistoryId, newStatus);

        Status = newStatus;
        Reason = "Order was successfully delivered.";

        _domainEvents.Add(new DeliverySucceededDomainEvent(OrderId, executionHistoryId));
    }

    public void MarkShipmentAsDeliveryFailed(string executionHistoryId)
    {
        if (Status != ShipmentStatus.DeliveringOrder)
            throw new InvalidOperationException($"Cannot change the current status ({Status}) to DeliveryFailed.");

        var newStatus = ShipmentStatus.DeliveryFailed;
        AddTrackingEvent(executionHistoryId, newStatus);

        Status = newStatus;
        Reason = "Order was unabled to deliver.";
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

    private bool IsAllowedToPickUpOrder(ShipmentStatus status, string? driverName) =>
        status == ShipmentStatus.DriverAssigned && !string.IsNullOrWhiteSpace(driverName);
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