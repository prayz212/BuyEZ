namespace OrderAPI.Application.Domain.Interfaces.Repositories;

public interface IOrderRepository
{
    Task AddAsync(Order order, CancellationToken cancellationToken);

    void Update(Order order);

    Task<Order?> GetByIdAsync(string id, CancellationToken cancellationToken);

    Task<bool> SaveChangesAsync(CancellationToken cancellationToken);
}