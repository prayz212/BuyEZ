using WarehouseWorker.Application.Domain;
using WarehouseWorker.Application.Infrastructure.Persistence;

using Shared.Common;

using Quartz;
using Microsoft.EntityFrameworkCore;

namespace WarehouseWorker.BackgroundJobs.Jobs;

public class NotifyShippingVendor : BaseJob<NotifyShippingVendor, ApplicationDbContext, PackageTrackingEvent>
{
    public NotifyShippingVendor(ILogger<NotifyShippingVendor> logger, ApplicationDbContext context) 
        : base(logger, context) { }

    public override async Task JobExecute(IJobExecutionContext context)
    {
        var packages = await _context.Packages.Where(p => p.Status == PackageStatus.ReadyToShip).ToListAsync();

        _logger.LogInformation("Found {PackageCount} packages need to notify", packages.Count);

        foreach (var package in packages)
        {
            _logger.LogInformation("Notifying shipping vendor for package: {@Package}", package);

            _events.Add(new(package, _executionHistory, PackageStatus.AwaitingShipment));
            package.UpdateStatus(PackageStatus.AwaitingShipment);
        }

        await _context.SaveChangesAsync();
    }
}