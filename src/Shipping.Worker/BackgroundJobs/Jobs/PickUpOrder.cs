using ShippingWorker.Application.Domain;
using ShippingWorker.Application.Infrastructure.Persistence;

using Shared.Common;

using Quartz;
using Microsoft.EntityFrameworkCore;

namespace ShippingWorker.BackgroundJobs.Jobs;

public class PickUpOrder : BaseJob<PickUpOrder, ApplicationDbContext, ShipmentTrackingEvent>
{
    public PickUpOrder(ILogger<PickUpOrder> logger, ApplicationDbContext context) 
        : base(logger, context) { }

    public override async Task JobExecute(IJobExecutionContext context)
    {
        var shipments = await _context.Shipments.Where(s => s.Status == ShipmentStatus.DriverAssigned).ToListAsync();

        _logger.LogInformation("Found {ShipmentCount} shipments need to pick up order", shipments.Count);

        foreach (var shipment in shipments)
        {
            _logger.LogInformation("Driver is picking up shipment: {@Shipment}", shipment);

            _events.Add(new(shipment, _executionHistory, ShipmentStatus.PickingUpOrder));
            shipment.UpdateStatus(ShipmentStatus.PickingUpOrder);
        }

        await _context.SaveChangesAsync();
    }
}