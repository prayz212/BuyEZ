using Shared.Infrastructure.Persistence;

using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Duende.IdentityServer.EntityFramework.DbContexts;

namespace Identity.Application.Infrastructure.Persistence;

public class PersistedGrantDbContextInitializer : IDbContextInitializer
{
    private readonly ILogger<PersistedGrantDbContextInitializer> _logger;
    private readonly PersistedGrantDbContext _context;

    public PersistedGrantDbContextInitializer(
        ILogger<PersistedGrantDbContextInitializer> logger, PersistedGrantDbContext context
    )
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
        _logger.LogInformation("No data seeding...");
        return Task.CompletedTask;
    }
}