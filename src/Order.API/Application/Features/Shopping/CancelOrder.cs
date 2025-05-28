using OrderAPI.Application.Domain.Interfaces.Repositories;

using Shared.Common.Exceptions;

using MediatR;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace OrderAPI.Application.Features.Shopping;


public record CancelOrderCommand(string? CurrentUserId, string OrderId) : IRequest;


public class CancelOrderCommandValidator : AbstractValidator<CancelOrderCommand>
{
    public CancelOrderCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("Order Id is required.");
    }
}


internal sealed class CancelOrderCommandHandler(
    ILogger<CancelOrderCommandHandler> logger,
    IOrderRepository orderRepository) 
    : IRequestHandler<CancelOrderCommand>
{
    private readonly ILogger<CancelOrderCommandHandler> _logger = logger;
    private readonly IOrderRepository _orderRepository = orderRepository;

    public async Task Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling request cancel order: {@Request}", request);

        // TODO: Refactor to reuse this validation
        if (string.IsNullOrWhiteSpace(request.CurrentUserId))
            throw new UnauthorizedAccessException("Invalid token.");

        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order is null)
            throw new NotFoundException($"Order with id: {request.OrderId} not found.");

        order.CancelOrder(request.CurrentUserId);

        _logger.LogInformation("Updating order to database: {@UpdatedOrder}", order);
        _orderRepository.Update(order);
        await _orderRepository.SaveChangesAsync(cancellationToken);
    }
}