using Shared.IntegrationEvents;

using MassTransit;
using Microsoft.Extensions.Logging;

namespace CatalogAPI.Application.Features.Consumers;

public class OrderPlacedConsumer(ILogger<OrderPlacedConsumer> logger) : IConsumer<OrderPlacedIntegrationEvent>
{
    private readonly ILogger<OrderPlacedConsumer> _logger = logger;

    public Task Consume(ConsumeContext<OrderPlacedIntegrationEvent> context)
    {
        _logger.LogInformation($"Consuming {nameof(OrderPlacedConsumer)} event...");

        return Task.CompletedTask;
    }
}