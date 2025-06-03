namespace ShippingWorker.Application.Domain.Interfaces.Repositories;

public interface IShipmentRepository
{
    Task<List<Shipment>> GetShipmentsByStatus(ShipmentStatus status);

    Task AddAsync(Shipment shipment);

    void Update(Shipment shipment);

    Task<bool> SaveChangesAsync();
}