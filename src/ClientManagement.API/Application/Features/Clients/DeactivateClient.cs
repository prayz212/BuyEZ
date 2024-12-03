using ClientManagementAPI.Application.Common.Exceptions;
using ClientManagementAPI.Application.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClientManagementAPI.Application.Features.Clients;

public record DeactivateClientCommand(string Id) : IRequest;


internal sealed class DeactivateClientCommandHandler(ApplicationDbContext context) : IRequestHandler<DeactivateClientCommand>
{
    private readonly ApplicationDbContext _context = context;

    public async Task Handle(DeactivateClientCommand request, CancellationToken cancellationToken)
    {
        var client = await _context.Clients.FirstOrDefaultAsync(c => c.Id == request.Id && c.IsActivated);
        if (client == null)
            throw new NotFoundException($"Client with id: {request.Id} is not found or already de-activated.");

        client.IsActivated = false;
        _context.Update(client);
        await _context.SaveChangesAsync(cancellationToken);
    }
}