using ShippingWorker.Application.Domain;
using ShippingWorker.Application.Domain.Interfaces.Repositories;

using Shared.Common;
using Shared.Common.Interfaces;

using Quartz;

namespace ShippingWorker.BackgroundJobs.Jobs;

public class PickUpOrder : BaseJob<PickUpOrder>
{
    private readonly IShipmentRepository _shipmentRepository;

    public PickUpOrder(
        ILogger<PickUpOrder> logger,
        IJobHistoryRepository jobRepository,
        IShipmentRepository shipmentRepository)
        : base(logger, jobRepository)
    {
        _shipmentRepository = shipmentRepository;
    }

    public override async Task JobExecute(IJobExecutionContext context)
    {
        var shipments = await _shipmentRepository.GetShipmentsByStatus(ShipmentStatus.DriverAssigned);

        _logger.LogInformation("Found {ShipmentCount} shipments need to pick up order", shipments.Count);

        var jobExecutionId = GetJobExecutionId();
        foreach (var shipment in shipments)
        {
            _logger.LogInformation("Driver is picking up shipment: {@Shipment}", shipment);

            shipment.PickUpOrder(jobExecutionId);
            _shipmentRepository.Update(shipment);
        }

        await _shipmentRepository.SaveChangesAsync();
    }
}