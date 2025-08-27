using CatalogAPI.Application.Domain.Events;

using Shared.Common.Models;

using MediatR;
using Microsoft.Extensions.Logging;

namespace CatalogAPI.Application.EventHandlers;

public class ProductDetailsUpdatedDomainEventHandler(ILogger<ProductDetailsUpdatedDomainEventHandler> logger) 
    : INotificationHandler<DomainEventNotification<ProductDetailsUpdatedDomainEvent>>
{
    private readonly ILogger<ProductDetailsUpdatedDomainEventHandler> _logger = logger;

    public Task Handle(DomainEventNotification<ProductDetailsUpdatedDomainEvent> notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Executing {nameof(ProductDetailsUpdatedDomainEventHandler)} event...");

        return Task.CompletedTask;
    }
}
