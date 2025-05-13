using Shared.Infrastructure.Persistence;

using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace WarehouseWorker.Application.Infrastructure.Persistence;

public class ApplicationDbContextInitializer : IDbContextInitializer
{
    private readonly ILogger<ApplicationDbContextInitializer> _logger;
    private readonly ApplicationDbContext _context;

    public ApplicationDbContextInitializer(ILogger<ApplicationDbContextInitializer> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task InitializeAsync()
    {
        try
        {
            _logger.LogInformation("Start migrating database...");
            await _context.Database.MigrateAsync();
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