using ShippingWorker.Application.Domain.Events;

using Shared.Common.Models;
using Shared.IntegrationEvents;

using MediatR;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace ShippingWorker.Application.EventHandlers;

public class DeliveryStartedDomainEventHandler(
    ILogger<DeliveryStartedDomainEventHandler> logger,
    ITopicProducer<DeliveryStartedIntegrationEvent> producer)
    : INotificationHandler<DomainEventNotification<DeliveryStartedDomainEvent>>
{
    private readonly ILogger<DeliveryStartedDomainEventHandler> _logger = logger;
    private readonly ITopicProducer<DeliveryStartedIntegrationEvent> _producer = producer;

    public async Task Handle(DomainEventNotification<DeliveryStartedDomainEvent> notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Executing {nameof(DeliveryStartedDomainEventHandler)} event...");

        var @event = notification.DomainEvent;
        await _producer.Produce(new DeliveryStartedIntegrationEvent
        {
            OrderId = @event.OrderId,
            JobId = @event.JobId
        });
    }
}