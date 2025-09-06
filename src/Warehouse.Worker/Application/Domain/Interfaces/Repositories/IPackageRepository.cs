namespace WarehouseWorker.Application.Domain.Interfaces.Repositories;

public interface IPackageRepository
{
    Task<List<Package>> GetPackagesByStatus(PackageStatus status);

    Task<Package?> GetPackageByOrderId(string orderId);

    Task AddAsync(Package package);

    void Update(Package package);

    Task<bool> SaveChangesAsync();
}