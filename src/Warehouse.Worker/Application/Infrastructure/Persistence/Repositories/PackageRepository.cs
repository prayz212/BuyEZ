using WarehouseWorker.Application.Domain;
using WarehouseWorker.Application.Domain.Interfaces.Repositories;

using Microsoft.EntityFrameworkCore;

namespace WarehouseWorker.Application.Infrastructure.Persistence.Repositories;

public class PackageRepository(ApplicationDbContext context) : IPackageRepository
{
    private readonly ApplicationDbContext _context = context;

    public async Task<List<Package>> GetPackagesByStatus(PackageStatus status)
    {
        return await _context.Packages.Where(p => p.Status == status).ToListAsync();
    }

    public async Task AddAsync(Package package)
    {
        await _context.AddAsync(package);
    }

    public void Update(Package package)
    {
        _context.Update(package);
    }
    
    public async Task<bool> SaveChangesAsync()
    {
        return (await _context.SaveChangesAsync()) > 0;
    }
}