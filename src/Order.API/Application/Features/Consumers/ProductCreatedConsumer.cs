using Shared.IntegrationEvents;

using MassTransit;
using Microsoft.Extensions.Logging;

namespace OrderAPI.Application.Features.Consumers;

public class ProductCreatedConsumer(ILogger<ProductCreatedConsumer> logger) : IConsumer<ProductCreatedIntegrationEvent>
{
    private readonly ILogger<ProductCreatedConsumer> _logger = logger;

    public Task Consume(ConsumeContext<ProductCreatedIntegrationEvent> context)
    {
        _logger.LogInformation($"Consuming {nameof(ProductCreatedConsumer)} event...");

        var message = context.Message;

        return Task.CompletedTask;
    }
}