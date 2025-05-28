using OrderAPI.Application.Domain;
using OrderAPI.Application.Domain.Interfaces.Repositories;

using Microsoft.EntityFrameworkCore;

namespace OrderAPI.Application.Infrastructure.Persistence.Repositories;

public class ProductRepository(ApplicationDbContext context) : IProductRepository
{
    private readonly ApplicationDbContext _context = context;

    public Task<List<ProductReference>> GetByIdsAsync(List<string> ids)
    {
        return _context.ProductReferences
            .Where(p => ids.Any(i => i == p.Id))
            .ToListAsync();
    }
}