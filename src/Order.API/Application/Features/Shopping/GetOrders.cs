using OrderAPI.Application.Domain;
using OrderAPI.Application.Infrastructure.Persistence;

using Shared.Common.Mappings;
using Shared.Common.Models;

using MediatR;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace OrderAPI.Application.Features.Shopping;


public record OrderCustomerInfoBriefResponse(string Name, string Address);

public record OrderBriefResponse(string Id, OrderCustomerInfoBriefResponse CustomerInfo, OrderStatus Status, double TotalAmount, DateTimeOffset CreatedAt);

public record GetOrdersPayload(int PageNumber = 1, int PageSize = 10);

public record GetOrdersQuery(string? CurrentUserId, GetOrdersPayload Payload) : IRequest<PaginatedList<OrderBriefResponse>>;


public class GetOrdersQueryValidator : AbstractValidator<GetOrdersQuery>
{
    public GetOrdersQueryValidator()
    {
        RuleFor(x => x.Payload)
            .NotNull().WithMessage("Request payload is required.")
            .SetValidator(new GetOrdersPayloadValidator());
    }

    class GetOrdersPayloadValidator : AbstractValidator<GetOrdersPayload>
    {
        public GetOrdersPayloadValidator()
        {
            RuleFor(x => x.PageNumber)
                .NotEmpty().WithMessage("PageNumber is required.")
                .GreaterThanOrEqualTo(1).WithMessage("PageNumber at least greater than or equal to 1.");

            RuleFor(x => x.PageSize)
                .NotEmpty().WithMessage("PageSize is required.")
                .GreaterThanOrEqualTo(1).WithMessage("PageSize at least greater than or equal to 1.");
        }
    }
}


internal sealed class GetOrdersQueryHandler(
    ILogger<GetOrdersQueryHandler> logger, 
    ApplicationDbContext context
) : IRequestHandler<GetOrdersQuery, PaginatedList<OrderBriefResponse>>
{
    private readonly ILogger<GetOrdersQueryHandler> _logger = logger;
    private readonly ApplicationDbContext _context = context;

    public async Task<PaginatedList<OrderBriefResponse>> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling request get orders: {@Request}", request);
        
        // TODO: Refactor to reuse this validation
        if (string.IsNullOrWhiteSpace(request.CurrentUserId))
            throw new UnauthorizedAccessException("Invalid token.");

        var payload = request.Payload;
        return await _context.Orders
            .Where(o => o.CreatedBy == request.CurrentUserId)
            .OrderBy(o => o.Created)
            .Select(o => ToDto(o))
            .PaginatedListAsync(payload.PageNumber, payload.PageSize, cancellationToken);
    }

    private static OrderBriefResponse ToDto(Order order) =>
        new (
            Id: order.Id,
            CustomerInfo: new(
                Name: order.CustomerName, 
                Address: order.CustomerAddress
            ),
            Status: order.Status,
            TotalAmount: order.TotalAmount,
            CreatedAt: order.Created
        );
}