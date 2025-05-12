using Shared.Domain;
using Shared.Infrastructure.Persistence;

using Quartz;
using Microsoft.Extensions.Logging;

namespace Shared.Common;

[DisallowConcurrentExecution]
public abstract class BaseJob<TJob, TDbContext, TTrackingEvent> : IJob where TDbContext : JobDbContext<TTrackingEvent>
{
    protected readonly ILogger<TJob> _logger;
    protected readonly TDbContext _context;
    protected readonly JobExecutionHistory<TTrackingEvent> _executionHistory;
    protected readonly List<TTrackingEvent> _events;

    public BaseJob(ILogger<TJob> logger, TDbContext context)
    {
        _logger = logger;
        _context = context;
        _executionHistory = new JobExecutionHistory<TTrackingEvent>(GetType().Name);
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