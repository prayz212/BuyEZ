using Shared.Common;

namespace WarehouseWorker.Application.Domain;

public class Package : AuditableEntity, IHasDomainEvent
{
    public string Id { get; private set; } = string.Empty;

    public PackageStatus Status { get; private set; } = PackageStatus.Pending;

    public string Reason { get; private set; } = string.Empty;

    public string OrderId { get; private set; } = string.Empty;

    public List<DomainEvent> DomainEvents { get; } = [];

    // Navigation Properties
    private List<PackageTrackingEvent> _trackingEvents = [];
    public IReadOnlyList<PackageTrackingEvent> TrackingEvents => _trackingEvents.AsReadOnly();

    // Constructors
    /*
        - By default, compiler will create a default parameterless constructor.
            => Compiler will NOT generate a parameterless constructor in case we already defined parameters constructor
     
        - Parameterless constructor for EF Core to instantiate the object with the data got from database
     */
    internal Package() { }

    public Package(string orderId, string? reason = null)
    {
        Id = Guid.NewGuid().ToString();
        OrderId = orderId;
        Reason = reason ?? "New order created";
    }

    public void UpdateStatus(PackageStatus status)
    {
        Status = status;

        if (status == PackageStatus.AwaitingShipment)
        {
            // TODO: add update order status event
            Console.WriteLine("UpdateOrderStatus event to DeliveringOrder");
        }
    }
}

public enum PackageStatus
{
    Pending = 1,
    ReadyToShip,
    AwaitingShipment,
    PackageDelivered,
    PackageReturned,
    ProductRefunded
}