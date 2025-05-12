using Shared.Domain;
using Microsoft.EntityFrameworkCore;

namespace Shared.Infrastructure.Persistence;

public class JobDbContext<TTrackingEvent> : DbContext
{
    public DbSet<JobExecutionHistory<TTrackingEvent>> JobExecutionHistories => Set<JobExecutionHistory<TTrackingEvent>>();

    public JobDbContext(DbContextOptions options) : base(options) { }
}