using Shared.Domain;
using Shared.Common.Interfaces;

using Quartz;
using Microsoft.Extensions.Logging;

namespace Shared.Common;

[DisallowConcurrentExecution]
public abstract class BaseJob<TJob> : IJob
{
    protected readonly ILogger<TJob> _logger;
    protected readonly IJobHistoryRepository _repository;
    private readonly JobExecutionHistory _executionHistory;

    public BaseJob(ILogger<TJob> logger, IJobHistoryRepository repository)
    {
        _logger = logger;
        _repository = repository;
        _executionHistory = new JobExecutionHistory(GetType().Name);
    }

    public abstract Task JobExecute(IJobExecutionContext context);

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("Executing {JobName} background job...", GetType().Name);

        try
        {
            await JobExecute(context);

            _executionHistory.ExecuteSuccess();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error encountered: {ErrorMessage}", ex.Message);
            _executionHistory.ExecuteFailed(ex.Message);
        }
        finally
        {
            await _repository.AddAsync(_executionHistory);
            await _repository.SaveChangesAsync();
        }

        _logger.LogInformation("Executed {JobName} background job.", GetType().Name);
    }

    protected string GetJobExecutionId()
    {
        return _executionHistory.Id;
    }
}