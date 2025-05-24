namespace CatalogAPI.Application.Domain.Interfaces.Repositories;

public interface IProductRepository
{
    Task AddAsync(Product product, CancellationToken cancellationToken);

    void Update(Product product);

    Task<Product?> GetById(string id, CancellationToken cancellationToken);

    Task<bool> SaveChangesAsync(CancellationToken cancellationToken);
}