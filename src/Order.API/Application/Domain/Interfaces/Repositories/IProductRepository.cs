namespace OrderAPI.Application.Domain.Interfaces.Repositories;

public interface IProductRepository
{
    Task<List<ProductReference>> GetByIdsAsync(List<string> ids);
}