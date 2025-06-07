using OrderAPI.Application.Domain.Interfaces.Repositories;

using Shared.IntegrationEvents;
using Shared.Common.Exceptions;

using MassTransit;
using Microsoft.Extensions.Logging;

namespace OrderAPI.Application.Features.Consumers;

public class DeliveryStartedConsumer(
    ILogger<DeliveryStartedConsumer> logger,
    IOrderRepository repository)
    : IConsumer<DeliveryStartedIntegrationEvent>
{
    private readonly ILogger<DeliveryStartedConsumer> _logger = logger;
    private readonly IOrderRepository _repository = repository;

    public async Task Consume(ConsumeContext<DeliveryStartedIntegrationEvent> context)
    {
        _logger.LogInformation("Consuming integration event DeliveryStarted: {@IntegrationEvent}", context.Message);

        var message = context.Message;

        try
        {
            var order = await _repository.GetByIdAsync(message.OrderId);
            if (order == null)
                throw new NotFoundException($"Order with id: {message.OrderId} not found.");

            var modifiedBy = $"bgj.shipping.{message.JobId}";
            _logger.LogInformation("Delivering order {OrderId}, modified by {ModifiedBy}", order.Id, modifiedBy);
            order.DeliverOrder(modifiedBy);

            _repository.Update(order);
            await _repository.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled Exception encountered: {ErrorMessage}", ex.Message);

            throw;
        }
    }
}