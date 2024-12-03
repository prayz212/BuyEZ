using ClientManagementAPI.Application.Common.Exceptions;
using ClientManagementAPI.Application.Domain.Clients;
using ClientManagementAPI.Application.Features.Clients.Shared.Dtos;
using ClientManagementAPI.Application.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClientManagementAPI.Application.Features.Clients;

public record GetClientRequest(string Id) : IRequest<ClientDetailResponse>;


internal sealed class GetClientRequestHandler(ApplicationDbContext context) : IRequestHandler<GetClientRequest, ClientDetailResponse>
{
    private readonly ApplicationDbContext _context = context;

    public async Task<ClientDetailResponse> Handle(GetClientRequest request, CancellationToken cancellationToken)
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