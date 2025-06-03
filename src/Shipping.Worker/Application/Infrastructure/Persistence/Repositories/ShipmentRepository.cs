using ShippingWorker.Application.Domain;
using ShippingWorker.Application.Domain.Interfaces.Repositories;

using Microsoft.EntityFrameworkCore;

namespace ShippingWorker.Application.Infrastructure.Persistence.Repositories;

public class ShipmentRepository(ApplicationDbContext context) : IShipmentRepository
{
    private readonly ApplicationDbContext _context = context;

    public async Task<List<Shipment>> GetShipmentsByStatus(ShipmentStatus status)
    {
        return await _context.Shipments.Where(s => s.Status == status).ToListAsync();
    }

    public async Task AddAsync(Shipment shipment)
    {
        await _context.Shipments.AddAsync(shipment);
    }

    public void Update(Shipment shipment)
    {
        _context.Update(shipment);
    }

    public async Task<bool> SaveChangesAsync()
    {
        return (await _context.SaveChangesAsync()) > 0;
    }
}