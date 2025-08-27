using CatalogAPI.Application.Domain.Events;

using Shared.Common.Models;

using MediatR;
using Microsoft.Extensions.Logging;

namespace CatalogAPI.Application.EventHandlers;

public class RestockThresholdReachedDomainEventHandler(ILogger<RestockThresholdReachedDomainEventHandler> logger) 
    : INotificationHandler<DomainEventNotification<RestockThresholdReachedDomainEvent>>
{
    private readonly ILogger<RestockThresholdReachedDomainEventHandler> _logger = logger;

    public Task Handle(DomainEventNotification<RestockThresholdReachedDomainEvent> notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Executing {nameof(RestockThresholdReachedDomainEvent)} event...");

        return Task.CompletedTask;
    }
}