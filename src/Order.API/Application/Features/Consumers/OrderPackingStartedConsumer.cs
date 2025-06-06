using OrderAPI.Application.Domain.Interfaces.Repositories;

using Shared.IntegrationEvents;
using Shared.Common.Exceptions;

using MassTransit;
using Microsoft.Extensions.Logging;

namespace OrderAPI.Application.Features.Consumers;

public class OrderPackingStartedConsumer(
    ILogger<OrderPackingStartedConsumer> logger,
    IOrderRepository repository)
    : IConsumer<OrderPackingStartedIntegrationEvent>
{
    private readonly ILogger<OrderPackingStartedConsumer> _logger = logger;
    private readonly IOrderRepository _repository = repository;

    public async Task Consume(ConsumeContext<OrderPackingStartedIntegrationEvent> context)
    {
        // TODO: Apply try catch
        _logger.LogInformation($"Consuming {nameof(OrderPackingStartedConsumer)} event...");

        var message = context.Message;

        var order = await _repository.GetByIdAsync(message.OrderId);

        if (order == null)
            throw new NotFoundException($"Order with id: {message.OrderId} not found.");

        order.PackOrder($"bgj.warehouse.{message.JobId}");
        _repository.Update(order);

        await _repository.SaveChangesAsync();
    }
}