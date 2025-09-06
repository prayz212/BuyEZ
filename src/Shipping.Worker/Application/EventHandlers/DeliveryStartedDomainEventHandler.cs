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
        _logger.LogInformation("Executing domain event handler DeliveryStarted: {@DomainEvent}", notification);

        var @event = notification.DomainEvent;
        var integrationEvent = new DeliveryStartedIntegrationEvent
        {
            OrderId = @event.OrderId,
            DriverName = @event.DriverName,
            JobId = @event.JobId
        };

        _logger.LogInformation("Producing integration event DeliveryStarted: {@IntegrationEvent}", integrationEvent);
        await _producer.Produce(integrationEvent);
    }
}