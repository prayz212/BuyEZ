using Shared.Domain;

namespace WarehouseWorker.Application.Domain;

public class PackageTrackingEvent
{
    public string PackageId { get; private set; } = string.Empty;

    public string ExecutionId { get; private set; } = string.Empty;

    public PackageStatus CurrentStatus { get; private set; }

    public PackageStatus NewStatus { get; private set; }

    // Navigation Properties
    public Package? Package { get; private set; }

    public JobExecutionHistory<PackageTrackingEvent>? ExecutionHistory { get; private set; }

    // Constructors
    /*
        - By default, compiler will create a default parameterless constructor.
            => Compiler will NOT generate a parameterless constructor in case we already defined parameters constructor
     
        - Parameterless constructor for EF Core to instantiate the object with the data got from database
     */
    internal PackageTrackingEvent() { }

    public PackageTrackingEvent(
        Package package,
        JobExecutionHistory<PackageTrackingEvent> executionHistory,
        PackageStatus newStatus)
    {
        PackageId = package.Id;
        ExecutionId = executionHistory.Id;
        CurrentStatus = package.Status;
        NewStatus = newStatus;
    }
}