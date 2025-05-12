namespace Shared.Domain;

public class JobExecutionHistory<TTrackingEvent>
{
    public string Id { get; private set; } = string.Empty;

    public string JobName { get; private set; } = string.Empty;

    public ExecutionStatus Status { get; private set; } = ExecutionStatus.Unknown;

    public DateTimeOffset ExecutedAt { get; private set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? CompletedAt { get; private set; }

    public string? ErrorMessage { get; private set; }

    // Navigation Properties
    private readonly List<TTrackingEvent> _trackingEvents = [];
    public IReadOnlyList<TTrackingEvent> TrackingEvents => _trackingEvents.AsReadOnly();

    // Constructors
    /*
        - By default, compiler will create a default parameterless constructor.
            => Compiler will NOT generate a parameterless constructor in case we already defined parameters constructor
     
        - Parameterless constructor for EF Core to instantiate the object with the data got from database
     */
    internal JobExecutionHistory() { }

    public JobExecutionHistory(string jobName)
    {
        Id = Guid.NewGuid().ToString();
        JobName = jobName;
        Status = ExecutionStatus.Unknown;
        ExecutedAt = DateTimeOffset.UtcNow;
    }

    public void ExecuteSuccess()
    {
        Status = ExecutionStatus.Success;
        CompletedAt = DateTimeOffset.UtcNow;
    }

    public void ExecuteFailed(string? errorMessage)
    {
        Status = ExecutionStatus.Failed;
        CompletedAt = DateTimeOffset.UtcNow;
        ErrorMessage = errorMessage ?? "Unknown error message.";
    }

    public void AddTrackingEvents(List<TTrackingEvent> @events)
    {
        _trackingEvents.AddRange(@events);
    }
}

public enum ExecutionStatus
{
    Unknown = 1,
    Success,
    Failed
}