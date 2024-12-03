using ClientManagementAPI.Application.Common.Exceptions;
using ClientManagementAPI.Application.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClientManagementAPI.Application.Features.Clients;

public record ActivateClientCommand(string Id) : IRequest;


internal sealed class ActivateClientCommandHandler(ApplicationDbContext context) : IRequestHandler<ActivateClientCommand>
{
    private readonly ApplicationDbContext _context = context;

    public async Task Handle(ActivateClientCommand request, CancellationToken cancellationToken)
    {
        var client = await _context.Clients.FirstOrDefaultAsync(c => c.Id == request.Id && !c.IsActivated);
        if (client == null)
            throw new NotFoundException($"Client with id: {request.Id} is not found or already de-activated.");

        client.IsActivated = true;
        _context.Update(client);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
