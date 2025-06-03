using Shared.Domain;

using Microsoft.EntityFrameworkCore;

namespace WarehouseWorker.Application.Infrastructure.Persistence;

public class BackgroundJobDbContext : DbContext
{
    public DbSet<JobExecutionHistory> JobExecutionHistories => Set<JobExecutionHistory>();

    public BackgroundJobDbContext(DbContextOptions options) : base(options) { }
}