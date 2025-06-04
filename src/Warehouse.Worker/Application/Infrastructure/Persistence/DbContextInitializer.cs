using Shared.Infrastructure.Persistence;

using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace WarehouseWorker.Application.Infrastructure.Persistence;

public class DbContextInitializer : IDbContextInitializer
{
    private readonly ILogger<DbContextInitializer> _logger;
    private readonly ApplicationDbContext _applicationDbContext;
    private readonly BackgroundJobDbContext _backgroundJobDbContext;

    public DbContextInitializer(
        ILogger<DbContextInitializer> logger,
        ApplicationDbContext applicationDbContext,
        BackgroundJobDbContext backgroundJobDbContext)
    {
        _logger = logger;
        _applicationDbContext = applicationDbContext;
        _backgroundJobDbContext = backgroundJobDbContext;
    }

    public async Task InitializeAsync()
    {
        try
        {
            _logger.LogInformation($"Start migrating database with {nameof(ApplicationDbContext)}...");
            await _applicationDbContext.Database.MigrateAsync();
            _logger.LogInformation("End migrating database.");

            _logger.LogInformation($"Start migrating database with {nameof(BackgroundJobDbContext)}...");
            await _backgroundJobDbContext.Database.MigrateAsync();
            _logger.LogInformation("End migrating database.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while initializing the database.");
            throw;
        }
    }

    public Task SeedAsync()
    {
        // Do nothing here since we not seed the database
        return Task.CompletedTask;
    }
}