namespace WarehouseWorker.Application.Domain;

public class PackageTrackingEvent
{
    public string PackageId { get; init; } = string.Empty;

    public string ExecutionId { get; init; } = string.Empty;

    public PackageStatus CurrentStatus { get; private set; }

    public PackageStatus NewStatus { get; private set; }

    // Constructors
    /*
        - By default, compiler will create a default parameterless constructor.
            => Compiler will NOT generate a parameterless constructor in case we already defined parameters constructor
     
        - Parameterless constructor for EF Core to instantiate the object with the data got from database
     */
    internal PackageTrackingEvent() { }

    private PackageTrackingEvent(
        string packageId,
        string executionHistoryId,
        PackageStatus currentStatus,
        PackageStatus newStatus)
    {
        PackageId = packageId;
        ExecutionId = executionHistoryId;
        CurrentStatus = currentStatus;
        NewStatus = newStatus;
    }

    public static PackageTrackingEvent CreateNew(
        Package package,
        string executionHistoryId,
        PackageStatus newStatus)
    {
        return new(
            package.Id,
            executionHistoryId,
            package.Status,
            newStatus);
    }
}