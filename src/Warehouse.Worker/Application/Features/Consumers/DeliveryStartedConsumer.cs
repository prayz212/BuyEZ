using WarehouseWorker.Application.Domain.Interfaces.Repositories;

using Shared.IntegrationEvents;
using Shared.Common.Exceptions;

using MassTransit;
using Microsoft.Extensions.Logging;

namespace WarehouseWorker.Application.Features.Consumers;

public class DeliveryStartedConsumer(
    ILogger<DeliveryStartedConsumer> logger,
    IPackageRepository repository) 
    : IConsumer<DeliveryStartedIntegrationEvent>
{
    private readonly ILogger<DeliveryStartedConsumer> _logger = logger;
    private readonly IPackageRepository _repository = repository;

    public async Task Consume(ConsumeContext<DeliveryStartedIntegrationEvent> context)
    {
        _logger.LogInformation("Consuming integration event DeliveryStarted: {@IntegrationEvent}", context.Message);

        var message = context.Message;

        try
        {
            var package = await _repository.GetPackageByOrderId(message.OrderId);
            if (package == null)
                throw new NotFoundException($"Package with OrderId: {message.OrderId} not found.");

            var modifiedBy = $"bgi.shipping.{message.JobId}";
            _logger.LogInformation("Package is collected by shipper {DriverName}: {@Package}", message.DriverName, package);
            package.CollectPackage(modifiedBy);

            _repository.Update(package);
            await _repository.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled Exception encountered: {ErrorMessage}", ex.Message);

            throw;
        }
    }
}