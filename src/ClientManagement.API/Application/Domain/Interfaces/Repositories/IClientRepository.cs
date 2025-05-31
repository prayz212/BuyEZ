namespace ClientManagementAPI.Application.Domain.Interfaces.Repositories;

public interface IClientRepository
{
    Task AddAsync(Client client, CancellationToken cancellationToken);

    Task<bool> CheckAliasNameExists(string aliasName, CancellationToken cancellationToken);

    Task<Client?> GetByIdAsync(string id, CancellationToken cancellationToken);

    void Update(Client client);

    Task<bool> SaveChangesAsync(CancellationToken cancellationToken);
}