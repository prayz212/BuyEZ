using ClientManagementAPI.Application.Domain.Interfaces.Repositories;

using Shared.Common.Exceptions;

using MediatR;
using Microsoft.Extensions.Logging;

namespace ClientManagementAPI.Application.Features.Administration;


public record ActivateClientPayload(string Id);

public record ActivateClientCommand(string? CurrentUserId, ActivateClientPayload Payload) : IRequest;


internal sealed class ActivateClientCommandHandler(ILogger<ActivateClientCommandHandler> logger, IClientRepository clientRepository) 
    : IRequestHandler<ActivateClientCommand>
{
    private readonly ILogger<ActivateClientCommandHandler> _logger = logger;
    private readonly IClientRepository _clientRepository = clientRepository;

    public async Task Handle(ActivateClientCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling request activate client: {@Request}", request);

        // TODO: refactor to reuse this validation
        if (string.IsNullOrWhiteSpace(request.CurrentUserId))
            throw new UnauthorizedAccessException("Invalid token.");

        var requestPayload = request.Payload;
        var client = await _clientRepository.GetByIdAsync(requestPayload.Id, cancellationToken);
        if (client == null)
            throw new NotFoundException($"Client with id: {requestPayload.Id} is not found or already de-activated.");

        client.Activate(request.CurrentUserId);

        _logger.LogInformation("Updating client to database: {@UpdatedClient}", client);
        _clientRepository.Update(client);

        await _clientRepository.SaveChangesAsync(cancellationToken);
    }
}
