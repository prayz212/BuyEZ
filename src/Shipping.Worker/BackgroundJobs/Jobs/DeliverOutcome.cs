using ShippingWorker.Application.Domain;
using ShippingWorker.Application.Domain.Interfaces.Repositories;

using Shared.Common;
using Shared.Common.Interfaces;

using Quartz;

namespace ShippingWorker.BackgroundJobs.Jobs;

public class DeliverOutcome : BaseJob<DeliverOutcome>
{
    public readonly IShipmentRepository _shipmentRepository;

    public DeliverOutcome(
        ILogger<DeliverOutcome> logger,
        IJobHistoryRepository jobRepository,
        IShipmentRepository shipmentRepository)
        : base(logger, jobRepository)
    {
        _shipmentRepository = shipmentRepository;
    }

    public override async Task JobExecute(IJobExecutionContext context)
    {
        var shipments = await _shipmentRepository.GetShipmentsByStatus(ShipmentStatus.DeliveringOrder);

        _logger.LogInformation("Found {ShipmentCount} shipments need to update delivery result", shipments.Count);

        var jobExecutionId = GetJobExecutionId();
        foreach (var shipment in shipments)
        {
            var deliveryResult = GetRandomDeliveryResult();
            _logger.LogInformation(deliveryResult
                ? "Succeeded to deliver shipment: {@Shipment}"
                : "Failed to deliver shipment: {@Shipment}"
                , shipment);

            if (deliveryResult)
                shipment.MarkShipmentAsDeliverySuccess(jobExecutionId);
            else
                shipment.MarkShipmentAsDeliveryFailed(jobExecutionId);

            _shipmentRepository.Update(shipment);
        }

        await _shipmentRepository.SaveChangesAsync();
    }

    private bool GetRandomDeliveryResult() => new Random().Next(1000) % 2 == 0;
}