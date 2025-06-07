using WarehouseWorker.Application.Domain;
using WarehouseWorker.Application.Domain.Interfaces.Repositories;

using Shared.IntegrationEvents;

using MassTransit;
using Microsoft.Extensions.Logging;

namespace WarehouseWorker.Application.Features.Consumers;

public class OrderPlacedConsumer(
    ILogger<OrderPlacedConsumer> logger,
    IPackageRepository repository) 
    : IConsumer<OrderPlacedIntegrationEvent>
{
    private readonly ILogger<OrderPlacedConsumer> _logger = logger;
    private readonly IPackageRepository _repository = repository;

    public async Task Consume(ConsumeContext<OrderPlacedIntegrationEvent> context)
    {
        _logger.LogInformation("Consuming integration event OrderPlaced: {@IntegrationEvent}", context.Message);

        var message = context.Message;

        try
        {
            var package = Package.CreateNew(message.OrderId, "New order placed.");
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