namespace WarehouseWorker.Application.Domain.Interfaces.Repositories;

public interface IPackageRepository
{
    Task<List<Package>> GetPackagesByStatus(PackageStatus status);

    Task AddAsync(Package package);

    void Update(Package package);

    Task<bool> SaveChangesAsync();
}