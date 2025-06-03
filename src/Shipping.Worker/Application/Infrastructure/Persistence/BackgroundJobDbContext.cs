using Shared.Domain;
using Shared.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;

namespace ShippingWorker.Application.Infrastructure.Persistence;

public class BackgroundJobDbContext : DbContext
{
    public DbSet<JobExecutionHistory> JobExecutionHistories => Set<JobExecutionHistory>();

    public BackgroundJobDbContext(DbContextOptions options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfiguration(new JobExecutionHistoryConfiguration());
        base.OnModelCreating(builder);
    }
}