using ClientManagementAPI.Application.Domain;
using ClientManagementAPI.Application.Infrastructure.Persistence;

using Shared.Common.Enums;
using Shared.Common.Exceptions;

using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClientManagementAPI.Application.Features.Administration;

public record ClientInfoResponse(string TenantId, string Name, DateTimeOffset ValidTo, ProductType[] ProductType, SubscriptionType Subscription, bool IsActive);

public record GetClientInfoQuery(string? Id) : IRequest<ClientInfoResponse>;

internal sealed class GetTenantInfoHandler(ApplicationDbContext context) : IRequestHandler<GetClientInfoQuery, ClientInfoResponse>
{
    private readonly ApplicationDbContext _context = context;

    public async Task<ClientInfoResponse> Handle(GetClientInfoQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Id))
            throw new ValidationException("Invalid client id.");

        var client = await _context.Clients
            .FirstOrDefaultAsync(c => c.Id == request.Id);

        if (client is null)
            throw new NotFoundException($"Client with id: {request.Id} not found.");

        return ToDto(client);
    }

    private static ClientInfoResponse ToDto(Client client) => new
    (
        client.Id,
        client.Name,
        client.ValidUntil,
        client.RegisteredProductType,
        client.SubscriptionType,
        client.IsActivated
    );

}