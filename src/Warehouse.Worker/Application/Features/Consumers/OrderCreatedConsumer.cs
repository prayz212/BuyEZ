using WarehouseWorker.Application.Domain;
using WarehouseWorker.Application.Domain.Interfaces.Repositories;

using Shared.IntegrationEvents;

using MassTransit;
using Microsoft.Extensions.Logging;

namespace WarehouseWorker.Application.Features.Consumers;

public class OrderCreatedConsumer(
    ILogger<OrderCreatedConsumer> logger,
    IPackageRepository repository) 
    : IConsumer<OrderCreatedIntegrationEvent>
{
    private readonly ILogger<OrderCreatedConsumer> _logger = logger;
    private readonly IPackageRepository _repository = repository;

    public async Task Consume(ConsumeContext<OrderCreatedIntegrationEvent> context)
    {
        _logger.LogInformation("Consuming integration event OrderCreated: {@IntegrationEvent}", context.Message);

        var message = context.Message;

        try
        {
            var package = Package.CreateNew(message.OrderId, "New order created.");
            _logger.LogInformation("Adding new package: {@Package}", package);

            await _repository.AddAsync(package);
            await _repository.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled Exception encountered: {ErrorMessage}", ex.Message);

            throw;
        }
    }
}