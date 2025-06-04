using WarehouseWorker.Application.Domain.Events;

using Shared.Common.Models;
using Shared.IntegrationEvents;

using MediatR;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace WarehouseWorker.Application.EventHandlers;

public class OrderPackedDomainEventHandler(
    ILogger<OrderPackedDomainEventHandler> logger,
    ITopicProducer<OrderPackedIntegrationEvent> producer)
    : INotificationHandler<DomainEventNotification<OrderPackedDomainEvent>>
{
    private readonly ILogger<OrderPackedDomainEventHandler> _logger = logger;
    private readonly ITopicProducer<OrderPackedIntegrationEvent> _producer = producer;

    public async Task Handle(DomainEventNotification<OrderPackedDomainEvent> notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Executing {nameof(OrderPackedDomainEventHandler)} event...");

        var @event = notification.DomainEvent;
        await _producer.Produce(new OrderPackedIntegrationEvent
        {
            OrderId = @event.OrderId
        });
    }
}