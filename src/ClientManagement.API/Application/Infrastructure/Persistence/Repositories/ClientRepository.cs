using ClientManagementAPI.Application.Domain;
using ClientManagementAPI.Application.Domain.Interfaces.Repositories;

using Microsoft.EntityFrameworkCore;

namespace ClientManagementAPI.Application.Infrastructure.Persistence.Repositories;

public class ClientRepository(ApplicationDbContext context) : IClientRepository
{
    private readonly ApplicationDbContext _context = context;

    public async Task AddAsync(Client client, CancellationToken cancellationToken)
    {
        await _context.Clients.AddAsync(client, cancellationToken);
    }

    public async Task<bool> CheckAliasNameExists(string aliasName, CancellationToken cancellationToken)
    {
        return await _context.Clients.AnyAsync(c => c.AliasName == aliasName, cancellationToken);
    }

    public async Task<Client?> GetByIdAsync(string id, CancellationToken cancellationToken)
    {
        return await _context.Clients
            .Include(c => c.Logo)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<bool> SaveChangesAsync(CancellationToken cancellationToken)
    {
        return (await _context.SaveChangesAsync(cancellationToken)) > 0;
    }

    public void Update(Client client)
    {
        _context.Update(client);
    }
}
