using ClientManagementAPI.Application.Common.Models;
using ClientManagementAPI.Application.Domain.Clients;
using ClientManagementAPI.Application.Infrastructure.Persistence;
using ClientManagementAPI.Application.Common.Mappings;
using FluentValidation;
using MediatR;

namespace ClientManagementAPI.Application.Features.Clients;

public record ClientBriefResponse(string Id, string Name, SubscriptionType SubscriptionType, DateTimeOffset ValidTo, bool IsActivated);

public record QueryClientRequest(int PageNumber = 1, int PageSize = 10) : IRequest<PaginatedList<ClientBriefResponse>>;


public class QueryClientRequestValidator : AbstractValidator<QueryClientRequest>
{
    public QueryClientRequestValidator()
    {
        RuleFor(x => x.PageNumber)
            .NotEmpty().WithMessage("PageNumber is required.")
            .GreaterThanOrEqualTo(1).WithMessage("PageNumber at least greater than or equal to 1.");

        RuleFor(x => x.PageSize)
            .NotEmpty().WithMessage("PageSize is required.")
            .GreaterThanOrEqualTo(1).WithMessage("PageSize at least greater than or equal to 1.");
    }
}


internal sealed class QueryClientRequestHandler(ApplicationDbContext context) : IRequestHandler<QueryClientRequest, PaginatedList<ClientBriefResponse>>
{
    private readonly ApplicationDbContext _context = context;

    public async Task<PaginatedList<ClientBriefResponse>> Handle(QueryClientRequest request, CancellationToken cancellationToken)
    {
        return await _context.Clients
            .OrderBy(c => c.Created)
            .Select(c => ToDto(c))
            .PaginatedListAsync(request.PageNumber, request.PageSize, cancellationToken);
    }

    private static ClientBriefResponse ToDto(Client client) => new
    (
        client.Id,
        client.Name,
        client.SubscriptionType,
        client.ValidUntil,
        client.IsActivated
    );
}