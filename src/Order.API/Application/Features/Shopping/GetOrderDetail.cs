using OrderAPI.Application.Domain.Shopping;
using OrderAPI.Application.Features.Shopping.Shared.Dtos;
using OrderAPI.Application.Infrastructure.Persistence;

using Shared.Common.Exceptions;

using MediatR;
using Microsoft.EntityFrameworkCore;

namespace OrderAPI.Application.Features.Shopping;


public record GetOrderDetailQuery(string? CurrentUserId, string OrderId) : IRequest<OrderDetailResponse>;


internal sealed class GetOrderDetailQueryHandler(ApplicationDbContext context) : IRequestHandler<GetOrderDetailQuery, OrderDetailResponse>
{
    private readonly ApplicationDbContext _context = context;

    public async Task<OrderDetailResponse> Handle(GetOrderDetailQuery request, CancellationToken cancellationToken)
    {
        // TODO: Refactor to reuse this validation
        if (string.IsNullOrWhiteSpace(request.CurrentUserId))
            throw new UnauthorizedAccessException("Invalid token.");

        var order = await _context.Orders
            .Include(o => EF.Property<List<OrderItem>>(o, "_orderItems")) // Since we cannot directly access to Backing fields
            .Include(o => EF.Property<List<OrderHistory>>(o, "_orderHistories")) // Since we cannot directly access to Backing fields
            .FirstOrDefaultAsync(o => o.Id == request.OrderId && o.CreatedBy == request.CurrentUserId);

        if (order is null)
            throw new NotFoundException($"Order with id: {request.OrderId} not found.");

        return Order.ToDto(order);
    }
}