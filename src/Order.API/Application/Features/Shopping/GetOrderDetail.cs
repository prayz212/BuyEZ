using OrderAPI.Application.Shared.Dtos;
using OrderAPI.Application.Infrastructure.Persistence;

using Shared.Common.Exceptions;

using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace OrderAPI.Application.Features.Shopping;


public record GetOrderDetailQuery(string? CurrentUserId, string OrderId) : IRequest<OrderDetailResponse>;


internal sealed class GetOrderDetailQueryHandler(ILogger<GetOrderDetailQueryHandler> logger, ApplicationDbContext context) : IRequestHandler<GetOrderDetailQuery, OrderDetailResponse>
{
    private readonly ILogger<GetOrderDetailQueryHandler> _logger = logger;
    private readonly ApplicationDbContext _context = context;

    public async Task<OrderDetailResponse> Handle(GetOrderDetailQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling request get order detail: {@Request}", request);
        
        // TODO: Refactor to reuse this validation
        if (string.IsNullOrWhiteSpace(request.CurrentUserId))
            throw new UnauthorizedAccessException("Invalid token.");

        var order = await _context.Orders
            .Include(o => o.OrderItems)
            .Include(o => o.OrderHistories)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId && o.CreatedBy == request.CurrentUserId);

        if (order is null)
            throw new NotFoundException($"Order with id: {request.OrderId} not found.");

        return order.ToDto();
    }
}