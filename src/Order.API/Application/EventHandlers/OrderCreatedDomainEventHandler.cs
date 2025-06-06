using OrderAPI.Application.Domain.Events;

using Shared.Common.Models;
using Shared.IntegrationEvents;

using MediatR;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace OrderAPI.Application.EventHandlers;

public class OrderCreatedDomainEventHandler(
    ILogger<OrderCreatedDomainEventHandler> logger,
    ITopicProducer<OrderCreatedIntegrationEvent> producer)
    : INotificationHandler<DomainEventNotification<OrderCreatedDomainEvent>>
{
    private readonly ILogger<OrderCreatedDomainEventHandler> _logger = logger;
    private readonly ITopicProducer<OrderCreatedIntegrationEvent> _producer = producer;

    public async Task Handle(DomainEventNotification<OrderCreatedDomainEvent> notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Executing {nameof(OrderCreatedDomainEventHandler)} event...");

        var @event = notification.DomainEvent;

        await _producer.Produce(new OrderCreatedIntegrationEvent
        {
            OrderId = @event.OrderId,
            TotalAmount = @event.TotalAmount,
            OrderItems = @event.OrderItems
                .Select(i => new KeyValuePair<string, int>(i.Id, i.Quantity))
                .ToDictionary(),
            CardNumber = @event.CardNumber,
            CardHolderName = @event.CardHolderName,
            SecurityCode = @event.SecurityCode,
            ExpirationDate = @event.ExpirationDate
        });
    }
}