using ClientManagementAPI.Application.Domain.Interfaces.Repositories;

using Shared.Common.Exceptions;

using MediatR;
using Microsoft.Extensions.Logging;

namespace ClientManagementAPI.Application.Features.Administration;


public record DeactivateClientPayload(string Id);

public record DeactivateClientCommand(string? CurrentUserId, DeactivateClientPayload Payload) : IRequest;


internal sealed class DeactivateClientCommandHandler(ILogger<DeactivateClientCommandHandler> logger, IClientRepository clientRepository) : IRequestHandler<DeactivateClientCommand>
{
    private readonly ILogger<DeactivateClientCommandHandler> _logger = logger;
    private readonly IClientRepository _clientRepository = clientRepository;

    public async Task Handle(DeactivateClientCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling request deactivate client: {@Request}", request);

        // TODO: refactor to reuse this validation
        if (string.IsNullOrWhiteSpace(request.CurrentUserId))
            throw new UnauthorizedAccessException("Invalid token.");

        var requestPayload = request.Payload;
        var client = await _clientRepository.GetByIdAsync(requestPayload.Id, cancellationToken);
        if (client == null)
            throw new NotFoundException($"Client with id: {requestPayload.Id} is not found or already de-activated.");

        client.Deactivate(request.CurrentUserId);

        _logger.LogInformation("Updating client to database: {@DeactivatedClient}", client);
        _clientRepository.Update(client);
        
        await _clientRepository.SaveChangesAsync(cancellationToken);
    }
}