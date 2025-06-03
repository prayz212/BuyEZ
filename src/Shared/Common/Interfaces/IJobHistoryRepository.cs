using Shared.Domain;

namespace Shared.Common.Interfaces;

public interface IJobHistoryRepository
{
    Task AddAsync(JobExecutionHistory job);

    Task<bool> SaveChangesAsync();
}