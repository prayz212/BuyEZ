using OrderAPI.Application.Domain.Events;

using Shared.Common.Models;
using Shared.IntegrationEvents;

using MediatR;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace OrderAPI.Application.EventHandlers;

public class OrderPlacedDomainEventHandler(
    ILogger<OrderPlacedDomainEventHandler> logger,
    ITopicProducer<OrderPlacedIntegrationEvent> producer)
    : INotificationHandler<DomainEventNotification<OrderPlacedDomainEvent>>
{
    private readonly ILogger<OrderPlacedDomainEventHandler> _logger = logger;
    private readonly ITopicProducer<OrderPlacedIntegrationEvent> _producer = producer;

    public async Task Handle(DomainEventNotification<OrderPlacedDomainEvent> notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Executing {nameof(OrderPlacedDomainEventHandler)} event...");

        var @event = notification.DomainEvent;
        await _producer.Produce(new OrderPlacedIntegrationEvent
        {
            OrderId = @event.OrderId
        });
    }
}