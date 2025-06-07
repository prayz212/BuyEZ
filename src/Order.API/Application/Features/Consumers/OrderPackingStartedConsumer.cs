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
        _logger.LogInformation("Consuming integration event OrderPackingStarted: {@IntegrationEvent}", context.Message);

        var message = context.Message;

        try
        {
            var order = await _repository.GetByIdAsync(message.OrderId);
            if (order == null)
                throw new NotFoundException($"Order with id: {message.OrderId} not found.");

            var modifiedBy = $"bgj.warehouse.{message.JobId}";
            _logger.LogInformation("Packing order {OrderId}, modified by {ModifiedBy}", order.Id, modifiedBy);
            order.PackOrder(modifiedBy);

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