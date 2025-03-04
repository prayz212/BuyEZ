using ClientManagementAPI.Application.Infrastructure.Persistence;

using Shared.Common.Exceptions;

using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClientManagementAPI.Application.Features.Administration;


public record DeactivateClientPayload(string Id);

public record DeactivateClientCommand(string? CurrentUserId, DeactivateClientPayload Payload) : IRequest;


internal sealed class DeactivateClientCommandHandler(ILogger<DeactivateClientCommandHandler> logger, ApplicationDbContext context) : IRequestHandler<DeactivateClientCommand>
{
    private readonly ILogger<DeactivateClientCommandHandler> _logger = logger;
    private readonly ApplicationDbContext _context = context;

    public async Task Handle(DeactivateClientCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling request deactivate client: {@Request}", request);

        // TODO: refactor to reuse this validation
        if (string.IsNullOrWhiteSpace(request.CurrentUserId))
            throw new UnauthorizedAccessException("Invalid token.");

        var requestPayload = request.Payload;
        var client = await _context.Clients.FirstOrDefaultAsync(c => c.Id == requestPayload.Id && c.IsActivated);
        if (client == null)
            throw new NotFoundException($"Client with id: {requestPayload.Id} is not found or already de-activated.");

        client.IsActivated = false;
        client.LastModifiedBy = request.CurrentUserId;

        _logger.LogInformation("Updating client to database: {@DeactivatedClient}", client);
        _context.Update(client);
        
        await _context.SaveChangesAsync(cancellationToken);
    }
}