using ShippingWorker.Application.Domain;
using ShippingWorker.Application.Domain.Interfaces.Repositories;

using Shared.IntegrationEvents;

using MassTransit;
using Microsoft.Extensions.Logging;

namespace ShippingWorker.Application.Features.Consumers;

public class OrderPackedConsumer(
    ILogger<OrderPackedConsumer> logger,
    IShipmentRepository repository)
    : IConsumer<OrderPackedIntegrationEvent>
{
    private readonly ILogger<OrderPackedConsumer> _logger = logger;
    private readonly IShipmentRepository _repository = repository;

    public async Task Consume(ConsumeContext<OrderPackedIntegrationEvent> context)
    {
        _logger.LogInformation("Consuming integration event OrderPacked: {@IntegrationEvent}", context.Message);

        var message = context.Message;

        try
        {
            var shipment = Shipment.CreateNew(message.OrderId, "New order packed.");
            _logger.LogInformation("Adding new shipment: {@Shipment}", shipment);

            await _repository.AddAsync(shipment);
            await _repository.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled Exception encountered: {ErrorMessage}", ex.Message);

            throw;
        }
    }
}