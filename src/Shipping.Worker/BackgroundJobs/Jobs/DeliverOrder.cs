using ShippingWorker.Application.Domain;
using ShippingWorker.Application.Domain.Interfaces.Repositories;

using Shared.Common;
using Shared.Common.Interfaces;

using Quartz;

namespace ShippingWorker.BackgroundJobs.Jobs;

public class DeliverOrder : BaseJob<DeliverOrder>
{
    private readonly IShipmentRepository _shipmentRepository;

    public DeliverOrder(
        ILogger<DeliverOrder> logger,
        IJobHistoryRepository jobRepository,
        IShipmentRepository shipmentRepository)
        : base(logger, jobRepository)
    {
        _shipmentRepository = shipmentRepository;
    }

    public override async Task JobExecute(IJobExecutionContext context)
    {
        var shipments = await _shipmentRepository.GetShipmentsByStatus(ShipmentStatus.PickingUpOrder);

        _logger.LogInformation("Found {ShipmentCount} shipments need to deliver order", shipments.Count);

        var jobExecutionId = GetJobExecutionId();
        foreach (var shipment in shipments)
        {
            _logger.LogInformation("Delivering shipment: {@Shipment}", shipment);

            shipment.DeliverOrder(jobExecutionId);
            _shipmentRepository.Update(shipment);
        }

        await _shipmentRepository.SaveChangesAsync();
    }
}