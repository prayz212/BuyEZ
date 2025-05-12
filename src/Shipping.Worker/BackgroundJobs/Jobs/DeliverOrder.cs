using ShippingWorker.Application.Domain;
using ShippingWorker.Application.Infrastructure.Persistence;

using Shared.Common;

using Quartz;
using Microsoft.EntityFrameworkCore;

namespace ShippingWorker.BackgroundJobs.Jobs;

public class DeliverOrder : BaseJob<DeliverOrder, ApplicationDbContext, ShipmentTrackingEvent>
{
    public DeliverOrder(ILogger<DeliverOrder> logger, ApplicationDbContext context)
        : base(logger, context) { }

    public override async Task JobExecute(IJobExecutionContext context)
    {
        var shipments = await _context.Shipments.Where(s => s.Status == ShipmentStatus.PickingUpOrder).ToListAsync();

        _logger.LogInformation("Found {ShipmentCount} shipments need to deliver order", shipments.Count);

        foreach (var shipment in shipments)
        {
            _logger.LogInformation("Delivering shipment: {@Shipment}", shipment);

            _events.Add(new(shipment, _executionHistory, ShipmentStatus.DeliveringOrder));
            shipment.UpdateStatus(ShipmentStatus.DeliveringOrder);
        }

        await _context.SaveChangesAsync();
    }
}