using ClientManagementAPI.Application.Shared.Dtos;
using ClientManagementAPI.Application.Infrastructure.Persistence;

using Shared.Common.Exceptions;

using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClientManagementAPI.Application.Features.Administration;


public record GetClientDetailQuery(string Id) : IRequest<ClientDetailResponse>;


internal sealed class GetClientDetailQueryHandler(ILogger<GetClientDetailQueryHandler> logger, ApplicationDbContext context) : IRequestHandler<GetClientDetailQuery, ClientDetailResponse>
{
    private readonly ILogger<GetClientDetailQueryHandler> _logger = logger;
    private readonly ApplicationDbContext _context = context;

    public async Task<ClientDetailResponse> Handle(GetClientDetailQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling request get client detail: {@Request}", request);
        
        if (string.IsNullOrWhiteSpace(request.Id))
            throw new ValidationException("Invalid client id.");

        var client = await _context.Clients
            .Include(c => c.Logo)
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (client is null)
            throw new NotFoundException($"Client with id: {request.Id} not found.");

        return client.ToDto();
    }
}