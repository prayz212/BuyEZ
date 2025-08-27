using CatalogAPI.Application.Domain.Events;

using Shared.Common.Models;
using Shared.IntegrationEvents;

using MediatR;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace CatalogAPI.Application.EventHandlers;

public class ProductCreatedDomainEventHandler(
    ILogger<ProductCreatedDomainEventHandler> logger,
    ITopicProducer<ProductCreatedIntegrationEvent> producer) 
    : INotificationHandler<DomainEventNotification<ProductCreatedDomainEvent>>
{
    private readonly ILogger<ProductCreatedDomainEventHandler> _logger = logger;
    private readonly ITopicProducer<ProductCreatedIntegrationEvent> _producer = producer;

    public async Task Handle(DomainEventNotification<ProductCreatedDomainEvent> notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Executing {nameof(ProductCreatedDomainEventHandler)} event...");

        var @event = notification.DomainEvent;

        await _producer.Produce(
            new ProductCreatedIntegrationEvent
            {
                ProductId = @event.ProductId,
                ProductName = @event.ProductName,
                ProductPrice = @event.ProductPrice
            }, cancellationToken);
    }
}
