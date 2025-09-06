using OrderAPI.Application.Domain.Events;

using Shared.Common.Models;
using Shared.IntegrationEvents;

using MediatR;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace OrderAPI.Application.EventHandlers;

public class OrderPlacedDomainEventHandler(
    ILogger<OrderPlacedDomainEventHandler> logger,
    ITopicProducer<OrderCreatedIntegrationEvent> producer)
    : INotificationHandler<DomainEventNotification<OrderPlacedDomainEvent>>
{
    private readonly ILogger<OrderPlacedDomainEventHandler> _logger = logger;
    private readonly ITopicProducer<OrderCreatedIntegrationEvent> _producer = producer;

    public async Task Handle(DomainEventNotification<OrderPlacedDomainEvent> notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Executing domain event handler OrderPlaced: {@DomainEvent}", notification);

        var @event = notification.DomainEvent;

        // TODO: Should trigger SAGA check here, then let SAGA trigger OrderCreatedIntegrationEvent
        // We just temporary bypass SAGA check due to not yet implement
        var integrationEvent = new OrderCreatedIntegrationEvent
        {
            OrderId = @event.OrderId,
            TotalAmount = @event.TotalAmount,
            OrderItems = @event.OrderItems
                .Select(i => new KeyValuePair<string, int>(i.Id, i.Quantity))
                .ToDictionary()
        };

        _logger.LogInformation("Producing integration event OrderCreated: {@IntegrationEvent}", integrationEvent);
        await _producer.Produce(integrationEvent);
    }
}