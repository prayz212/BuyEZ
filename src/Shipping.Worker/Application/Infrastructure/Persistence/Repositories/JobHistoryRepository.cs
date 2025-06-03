using Shared.Domain;
using Shared.Common.Interfaces;

namespace ShippingWorker.Application.Infrastructure.Persistence.Repositories;

public class JobHistoryRepository(BackgroundJobDbContext context) : IJobHistoryRepository
{
    private readonly BackgroundJobDbContext _context = context;

    public async Task AddAsync(JobExecutionHistory job)
    {
        await _context.AddAsync(job);
    }

    public async Task<bool> SaveChangesAsync()
    {
        return (await _context.SaveChangesAsync()) > 0;
    }
}