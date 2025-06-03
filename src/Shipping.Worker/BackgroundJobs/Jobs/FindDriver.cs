using ShippingWorker.Application.Domain;
using ShippingWorker.Application.Domain.Interfaces.Repositories;

using Shared.Common;
using Shared.Common.Interfaces;

using Quartz;

namespace ShippingWorker.BackgroundJobs.Jobs;

public class FindDriver : BaseJob<FindDriver>
{
    private readonly IShipmentRepository _shipmentRepository;

    public FindDriver(
        ILogger<FindDriver> logger,
        IJobHistoryRepository jobRepository,
        IShipmentRepository shipmentRepository)
        : base(logger, jobRepository)
    {
        _shipmentRepository = shipmentRepository;
    }

    public override async Task JobExecute(IJobExecutionContext context)
    {
        var shipments = await _shipmentRepository.GetShipmentsByStatus(ShipmentStatus.FindingDriver);

        _logger.LogInformation("Found {ShipmentCount} shipments need to assign driver", shipments.Count);

        var jobExecutionId = GetJobExecutionId();
        // TODO: Make driver name randomly
        var driverName = "Lee Wan Shi";
        foreach (var shipment in shipments)
        {
            _logger.LogInformation("Assigning driver to shipment: {@Shipment}", shipment);

            shipment.AssignDriver(driverName, jobExecutionId);
            _shipmentRepository.Update(shipment);
        }

        await _shipmentRepository.SaveChangesAsync();
    }
}
