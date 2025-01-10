using ClientManagementAPI.Application.Domain;
using ClientManagementAPI.Application.Infrastructure.Persistence;

using Shared.Common.Models;
using Shared.Common.Mappings;

using FluentValidation;
using MediatR;

namespace ClientManagementAPI.Application.Features.Administration;


public record ClientBriefResponse(string Id, string Name, SubscriptionType SubscriptionType, DateTimeOffset ValidTo, bool IsActivated);

public record GetClientsQuery(int PageNumber = 1, int PageSize = 10) : IRequest<PaginatedList<ClientBriefResponse>>;


public class GetClientsQueryValidator : AbstractValidator<GetClientsQuery>
{
    public GetClientsQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .NotEmpty().WithMessage("PageNumber is required.")
            .GreaterThanOrEqualTo(1).WithMessage("PageNumber at least greater than or equal to 1.");

        RuleFor(x => x.PageSize)
            .NotEmpty().WithMessage("PageSize is required.")
            .GreaterThanOrEqualTo(1).WithMessage("PageSize at least greater than or equal to 1.");
    }
}


internal sealed class GetClientsQueryHandler(ApplicationDbContext context) : IRequestHandler<GetClientsQuery, PaginatedList<ClientBriefResponse>>
{
    private readonly ApplicationDbContext _context = context;

    public async Task<PaginatedList<ClientBriefResponse>> Handle(GetClientsQuery request, CancellationToken cancellationToken)
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