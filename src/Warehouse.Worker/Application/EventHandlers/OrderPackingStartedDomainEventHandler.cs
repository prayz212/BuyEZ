using WarehouseWorker.Application.Domain.Events;

using Shared.Common.Models;
using Shared.IntegrationEvents;

using MediatR;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace WarehouseWorker.Application.EventHandlers;

public class OrderPackingStartedDomainEventHandler(
    ILogger<OrderPackingStartedDomainEventHandler> logger,
    ITopicProducer<OrderPackingStartedIntegrationEvent> producer)
    : INotificationHandler<DomainEventNotification<OrderPackingStartedDomainEvent>>
{
    private readonly ILogger<OrderPackingStartedDomainEventHandler> _logger = logger;
    private readonly ITopicProducer<OrderPackingStartedIntegrationEvent> _producer = producer;

    public async Task Handle(DomainEventNotification<OrderPackingStartedDomainEvent> notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Executing domain event handler OrderPackingStarted: {@DomainEvent}", notification);

        var @event = notification.DomainEvent;
        var integrationEvent = new OrderPackingStartedIntegrationEvent
        {
            OrderId = @event.OrderId,
            JobId = @event.JobId
        };

        _logger.LogInformation("Producing integration event OrderPackingStarted: {@IntegrationEvent}", integrationEvent);
        await _producer.Produce(integrationEvent);
    }
}