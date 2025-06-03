namespace ShippingWorker.Application.Domain;

public class ShipmentTrackingEvent
{
    public string ShipmentId { get; init; } = string.Empty;

    public string ExecutionId { get; init; } = string.Empty;

    public ShipmentStatus CurrentStatus { get; private set; }

    public ShipmentStatus NewStatus { get; private set; }

    // Constructors
    /*
        - By default, compiler will create a default parameterless constructor.
            => Compiler will NOT generate a parameterless constructor in case we already defined parameters constructor
     
        - Parameterless constructor for EF Core to instantiate the object with the data got from database
     */
    internal ShipmentTrackingEvent() { }

    private ShipmentTrackingEvent(
        string shipmentId,
        string executionHistoryId,
        ShipmentStatus currentStatus,
        ShipmentStatus newStatus)
    {
        ShipmentId = shipmentId;
        ExecutionId = executionHistoryId;
        CurrentStatus = currentStatus;
        NewStatus = newStatus;
    }

    public static ShipmentTrackingEvent CreateNew(
        Shipment shipment,
        string executionHistoryId,
        ShipmentStatus newStatus)
    {
        return new(
            shipment.Id,
            executionHistoryId,
            shipment.Status,
            newStatus);
    }
}