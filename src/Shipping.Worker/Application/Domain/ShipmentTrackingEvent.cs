namespace ShippingWorker.Application.Domain;

public class ShipmentTrackingEvent
{
    public string ShipmentId { get; private set; } = string.Empty;

    public string ExecutionId { get; private set; } = string.Empty;

    public ShipmentStatus CurrentStatus { get; private set; }

    public ShipmentStatus NewStatus { get; private set; }

    // Navigation Properties
    public Shipment? Shipment { get; private set; }

    public JobExecutionHistory? ExecutionHistory { get; private set; }

    // Constructors
    /*
        - By default, compiler will create a default parameterless constructor.
            => Compiler will NOT generate a parameterless constructor in case we already defined parameters constructor
     
        - Parameterless constructor for EF Core to instantiate the object with the data got from database
     */
    internal ShipmentTrackingEvent() { }

    public ShipmentTrackingEvent(
        Shipment shipment,
        JobExecutionHistory executionHistory,
        ShipmentStatus newStatus)
    {
        ShipmentId = shipment.Id;
        ExecutionId = executionHistory.Id;
        CurrentStatus = shipment.Status;
        NewStatus = newStatus;
    }
}