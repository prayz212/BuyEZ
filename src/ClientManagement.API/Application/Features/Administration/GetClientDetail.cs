using ClientManagementAPI.Application.Domain;
using ClientManagementAPI.Application.Shared.Dtos;
using ClientManagementAPI.Application.Infrastructure.Persistence;

using Shared.Common.Exceptions;

using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClientManagementAPI.Application.Features.Administration;


public record GetClientDetailQuery(string Id) : IRequest<ClientDetailResponse>;


internal sealed class GetClientDetailQueryHandler(ApplicationDbContext context) : IRequestHandler<GetClientDetailQuery, ClientDetailResponse>
{
    private readonly ApplicationDbContext _context = context;

    public async Task<ClientDetailResponse> Handle(GetClientDetailQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Id))
            throw new ValidationException("Invalid client id.");

        var client = await _context.Clients
            .Include(c => c.Logo)
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (client is null)
            throw new NotFoundException($"Client with id: {request.Id} not found.");

        return Client.ToDto(client);
    }
}