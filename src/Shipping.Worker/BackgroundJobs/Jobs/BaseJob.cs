using ShippingWorker.Application.Domain;
using ShippingWorker.Application.Infrastructure.Persistence;

using Quartz;

namespace ShippingWorker.BackgroundJobs.Jobs;

[DisallowConcurrentExecution]
public abstract class BaseJob<TJob> : IJob
{
    protected readonly ILogger<TJob> _logger;
    protected readonly ApplicationDbContext _context;
    protected readonly JobExecutionHistory _executionHistory;
    protected readonly List<ShipmentTrackingEvent> _events;

    public BaseJob(ILogger<TJob> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
        _executionHistory = new JobExecutionHistory(GetType().Name);
        _events = [];
    }

    public abstract Task JobExecute(IJobExecutionContext context);

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("Executing {JobName} background job...", GetType().Name);

        try 
        {
            await JobExecute(context);
            
            if (_events.Any()) _executionHistory.AddTrackingEvent(_events);
            _executionHistory.ExecuteSuccess();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error encountered: {ErrorMessage}", ex.Message);
            _executionHistory.ExecuteFailed(ex.Message);
        }
        finally
        {
            await _context.JobExecutionHistories.AddAsync(_executionHistory);
            await _context.SaveChangesAsync();
        }

        _logger.LogInformation("Executed {JobName} background job.", GetType().Name);
    }
}