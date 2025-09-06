using ShippingWorker.Application.Domain.Events;

using Shared.Common.Models;
using Shared.IntegrationEvents;

using MediatR;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace ShippingWorker.Application.EventHandlers;

public class DeliverySucceededDomainEventHandler(
    ILogger<DeliverySucceededDomainEventHandler> logger,
    ITopicProducer<DeliverySucceededIntegrationEvent> producer)
    : INotificationHandler<DomainEventNotification<DeliverySucceededDomainEvent>>
{
    private readonly ILogger<DeliverySucceededDomainEventHandler> _logger = logger;
    private readonly ITopicProducer<DeliverySucceededIntegrationEvent> _producer = producer;

    public async Task Handle(DomainEventNotification<DeliverySucceededDomainEvent> notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Executing domain event handler DeliverySucceeded: {@DomainEvent}", notification);

        var @event = notification.DomainEvent;
        var integrationEvent = new DeliverySucceededIntegrationEvent
        {
            OrderId = @event.OrderId,
            JobId = @event.JobId
        };

        _logger.LogInformation("Producing integration event DeliverySucceeded: {@IntegrationEvent}", integrationEvent);
        await _producer.Produce(integrationEvent);
    }
}