using OrderAPI.Application.Domain.Interfaces.Repositories;

using Shared.IntegrationEvents;
using Shared.Common.Exceptions;

using MassTransit;
using Microsoft.Extensions.Logging;

namespace OrderAPI.Application.Features.Consumers;

public class DeliverySucceededConsumer(
    ILogger<DeliverySucceededConsumer> logger,
    IOrderRepository repository)
    : IConsumer<DeliverySucceededIntegrationEvent>
{
    private readonly ILogger<DeliverySucceededConsumer> _logger = logger;
    private readonly IOrderRepository _repository = repository;

    public async Task Consume(ConsumeContext<DeliverySucceededIntegrationEvent> context)
    {
        _logger.LogInformation("Consuming integration event DeliverySucceeded: {@IntegrationEvent}", context.Message);

        var message = context.Message;

        try
        {
            var order = await _repository.GetByIdAsync(message.OrderId);
            if (order == null)
                throw new NotFoundException($"Order with id: {message.OrderId} not found.");

            var modifiedBy = $"bgj.shipping.{message.JobId}";
            _logger.LogInformation("Delivered order {OrderId}, modified by {ModifiedBy}", order.Id, modifiedBy);
            order.MarkOrderAsDelivered(modifiedBy);

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