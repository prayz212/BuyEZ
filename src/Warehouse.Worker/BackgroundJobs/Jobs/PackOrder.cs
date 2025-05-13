using WarehouseWorker.Application.Domain;
using WarehouseWorker.Application.Infrastructure.Persistence;

using Shared.Common;

using Quartz;
using Microsoft.EntityFrameworkCore;

namespace WarehouseWorker.BackgroundJobs.Jobs;

public class PackOrder : BaseJob<PackOrder, ApplicationDbContext, PackageTrackingEvent>
{
    public PackOrder(ILogger<PackOrder> logger, ApplicationDbContext context) 
        : base(logger, context) { }

    public override async Task JobExecute(IJobExecutionContext context)
    {
        var packages = await _context.Packages.Where(p => p.Status == PackageStatus.Pending).ToListAsync();

        _logger.LogInformation("Found {PackageCount} orders need to pack", packages.Count);

        foreach (var package in packages)
        {
            _logger.LogInformation("Packing order: {@Package}", package);

            _events.Add(new(package, _executionHistory, PackageStatus.ReadyToShip));
            package.UpdateStatus(PackageStatus.ReadyToShip);
        }

        await _context.SaveChangesAsync();
    }
}