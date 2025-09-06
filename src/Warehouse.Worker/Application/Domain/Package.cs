using WarehouseWorker.Application.Domain.Events;

using Shared.Common;
using Shared.Common.Interfaces;

namespace WarehouseWorker.Application.Domain;

public class Package : AuditableEntity, IAggregateRoot
{
    public string Id { get; init; } = string.Empty;

    public PackageStatus Status { get; private set; } = PackageStatus.Pending;

    public string Reason { get; private set; } = string.Empty;

    public string OrderId { get; private set; } = string.Empty;

    // Navigation Properties
    private List<PackageTrackingEvent> _trackingEvents = [];
    public IReadOnlyList<PackageTrackingEvent> TrackingEvents => _trackingEvents.AsReadOnly();

    // Domain Events property
    private readonly List<DomainEvent> _domainEvents = [];
    public IReadOnlyList<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    // Constructors
    /*
        - By default, compiler will create a default parameterless constructor.
            => Compiler will NOT generate a parameterless constructor in case we already defined parameters constructor
     
        - Parameterless constructor for EF Core to instantiate the object with the data got from database
     */
    internal Package() { }

    private Package(string orderId, string reason)
    {
        Id = Guid.NewGuid().ToString();
        OrderId = orderId;
        Status = PackageStatus.Pending;
        Reason = reason;
    }

    public static Package CreateNew(string orderId, string? reason = default)
    {
        return new(orderId, reason ?? "New order created.");
    }

    public void PackOrder(string executionHistoryId)
    {
        if (Status != PackageStatus.Pending)
            throw new InvalidOperationException($"Cannot change the current status ({Status}) to Packing.");

        var newStatus = PackageStatus.Packing;
        AddTrackingEvent(executionHistoryId, newStatus);

        Status = newStatus;
        Reason = "Order is now packing.";

        _domainEvents.Add(new OrderPackingStartedDomainEvent(OrderId, executionHistoryId));
    }

    public void MarkOrderReadyForShipment(string executionHistoryId)
    {
        if (Status != PackageStatus.Packing)
            throw new InvalidOperationException($"Cannot change the current status ({Status}) to AwaitingShipment.");

        var newStatus = PackageStatus.AwaitingShipment;
        AddTrackingEvent(executionHistoryId, newStatus);

        Status = newStatus;
        Reason = "Package is waiting for shipping vendor to collect.";

        _domainEvents.Add(new OrderPackedDomainEvent(OrderId));
    }

    public void CollectPackage(string modifiedBy)
    {
        if (Status != PackageStatus.AwaitingShipment)
            throw new InvalidOperationException($"Cannot change the current status ({Status}) to PackageCollected.");

        var newStatus = PackageStatus.PackageCollected;
        AddTrackingEvent(modifiedBy, newStatus);

        Status = newStatus;
        Reason = "Package is collected for shipment.";
    }

    private void AddTrackingEvent(string executionHistoryId, PackageStatus newStatus)
    {
        _trackingEvents.Add(
            PackageTrackingEvent.CreateNew(
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

public enum PackageStatus
{
    Pending = 1,
    Packing,
    AwaitingShipment,
    PackageCollected,
    PackageDelivered,
    PackageReturned,
    ProductRefunded
}