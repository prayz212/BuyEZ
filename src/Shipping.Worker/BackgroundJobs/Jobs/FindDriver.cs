using ShippingWorker.Application.Domain;
using ShippingWorker.Application.Infrastructure.Persistence;

using Quartz;
using Microsoft.EntityFrameworkCore;

namespace ShippingWorker.BackgroundJobs.Jobs;

public class FindDriver : BaseJob<FindDriver>
{
    public FindDriver(ILogger<FindDriver> logger, ApplicationDbContext context) 
        : base(logger, context) { }

    public override async Task JobExecute(IJobExecutionContext context)
    {
        var shipments = await _context.Shipments.Where(s => s.Status == ShipmentStatus.FindingDriver).ToListAsync();

        _logger.LogInformation("Found {ShipmentCount} shipments need to assign driver", shipments.Count);

        foreach (var shipment in shipments)
        {
            _logger.LogInformation("Assigning driver to shipment {ShipmentId}", shipment.Id);

            _events.Add(new(shipment, _executionHistory, ShipmentStatus.DriverAssigned));
            shipment.UpdateStatus(ShipmentStatus.DriverAssigned);
        }
    }
}
