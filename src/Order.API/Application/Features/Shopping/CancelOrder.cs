using OrderAPI.Application.Domain;
using OrderAPI.Application.Infrastructure.Persistence;

using Shared.Common.Exceptions;
using ValidationException = Shared.Common.Exceptions.ValidationException;

using MediatR;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

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


internal sealed class CancelOrderCommandHandler(ApplicationDbContext context) : IRequestHandler<CancelOrderCommand>
{
    private readonly ApplicationDbContext _context = context;

    public async Task Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        // TODO: Refactor to reuse this validation
        if (string.IsNullOrWhiteSpace(request.CurrentUserId))
            throw new UnauthorizedAccessException("Invalid token.");

        var order = await _context.Orders.FirstOrDefaultAsync(o => 
            o.Id == request.OrderId && o.CreatedBy == request.CurrentUserId);
        if (order is null)
            throw new NotFoundException($"Order with id: {request.OrderId} not found.");

        if (!IsAllowedToCancelOrder(order.Status))
            throw new ValidationException("Order can't be cancelled.");

        order.LastModifiedBy = request.CurrentUserId;
        order.UpdateOrderStatus(OrderStatus.Cancelled);

        _context.Orders.Update(order);
        await _context.SaveChangesAsync(cancellationToken);

        // TODO: Publish cancelled order event
    }

    private bool IsAllowedToCancelOrder(OrderStatus status) => 
        status == OrderStatus.Pending;
}