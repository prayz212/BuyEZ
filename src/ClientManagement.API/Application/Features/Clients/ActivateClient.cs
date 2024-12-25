using ClientManagementAPI.Application.Infrastructure.Persistence;

using Shared.Common.Exceptions;

using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClientManagementAPI.Application.Features.Clients;

public record ActivateClientRequest(string Id);

public record ActivateClientCommand(string? CurrentUserId, ActivateClientRequest Payload) : IRequest;


internal sealed class ActivateClientCommandHandler(ApplicationDbContext context) : IRequestHandler<ActivateClientCommand>
{
    private readonly ApplicationDbContext _context = context;

    public async Task Handle(ActivateClientCommand request, CancellationToken cancellationToken)
    {
        // TODO: refactor to reuse this validation
        if (string.IsNullOrWhiteSpace(request.CurrentUserId))
            throw new UnauthorizedAccessException("Invalid token.");

        var requestPayload = request.Payload;
        var client = await _context.Clients.FirstOrDefaultAsync(c => c.Id == requestPayload.Id && !c.IsActivated);
        if (client == null)
            throw new NotFoundException($"Client with id: {requestPayload.Id} is not found or already de-activated.");

        client.IsActivated = true;
        client.LastModifiedBy = request.CurrentUserId;
        _context.Update(client);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
