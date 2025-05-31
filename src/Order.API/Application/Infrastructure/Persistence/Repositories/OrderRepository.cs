using OrderAPI.Application.Domain;
using OrderAPI.Application.Domain.Interfaces.Repositories;

using Microsoft.EntityFrameworkCore;

namespace OrderAPI.Application.Infrastructure.Persistence.Repositories;

public class OrderRepository(ApplicationDbContext context) : IOrderRepository
{
    private readonly ApplicationDbContext _context = context;

    public async Task AddAsync(Order order, CancellationToken cancellationToken)
    {
        await _context.Orders.AddAsync(order, cancellationToken);
    }

    public Task<Order?> GetByIdAsync(string id, CancellationToken cancellationToken)
    {
        return _context.Orders
            .Include(o => o.OrderItems)
            .Include(o => o.OrderHistories)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    }

    public async Task<bool> SaveChangesAsync(CancellationToken cancellationToken)
    {
        return (await _context.SaveChangesAsync()) > 0;
    }

    public void Update(Order order)
    {
        _context.Update(order);
    }
}