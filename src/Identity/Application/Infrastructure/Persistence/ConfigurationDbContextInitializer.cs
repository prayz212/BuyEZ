using Identity.Application.Common;

using Shared.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.EntityFramework.Mappers;

namespace Identity.Application.Infrastructure.Persistence;

public class ConfigurationDbContextInitializer : IDbContextInitializer
{
    private readonly ILogger<ConfigurationDbContextInitializer> _logger;
    private readonly ConfigurationDbContext _context;

    public ConfigurationDbContextInitializer (
        ILogger<ConfigurationDbContextInitializer> logger,
        ConfigurationDbContext context
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

    public async Task SeedAsync()
    {
        try
        {
            _logger.LogInformation("Start seeding database...");
            await TrySeedAsync();
            _logger.LogInformation("End seeding database.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

    private async Task TrySeedAsync()
    {
        /*  Seeding identity resources   */
        await SeedIdentityResources();

        /*  Seeding api scopes   */
        await SeedApiScopes();

        /*  Seeding api scopes   */
        await SeedApiResources();

        /*  Seeding clients   */
        await SeedClients();
    }

    private async Task SeedIdentityResources()
    {
        if (await _context.IdentityResources.AnyAsync())
        {
            _logger.LogInformation("Identity resources already exists.");
            return;
        }

        _logger.LogInformation("Seeding identity resources...");
        foreach (var resource in Config.IdentityResources)
        {
            _context.IdentityResources.Add(resource.ToEntity());
        }
        
        _context.SaveChanges();   

        _logger.LogInformation("Seeded {total} identity resources.", Config.IdentityResources.Count());
    }

    private async Task SeedApiScopes()
    {
        if (await _context.ApiScopes.AnyAsync())
        {
            _logger.LogInformation("Api scopes already exists.");
            return;
        }

        _logger.LogInformation("Seeding api scopes...");
        foreach (var resource in Config.ApiScopes)
        {
            _context.ApiScopes.Add(resource.ToEntity());
        }
        
        _context.SaveChanges();   

        _logger.LogInformation("Seeded {total} api scopes.", Config.ApiScopes.Count());
    }

    private async Task SeedApiResources()
    {
        if (await _context.ApiResources.AnyAsync())
        {
            _logger.LogInformation("Api resources already exists.");
            return;
        }

        _logger.LogInformation("Seeding api resources...");
        foreach (var resource in Config.ApiResources)
        {
            _context.ApiResources.Add(resource.ToEntity());
        }
        
        _context.SaveChanges();   

        _logger.LogInformation("Seeded {total} api resources.", Config.ApiResources.Count());
    }

    private async Task SeedClients()
    {
        if (await _context.Clients.AnyAsync())
        {
            _logger.LogInformation("Clients already exists.");
            return;
        }

        _logger.LogInformation("Seeding clients...");
        foreach (var resource in Config.Clients)
        {
            _context.Clients.Add(resource.ToEntity());
        }
        
        _context.SaveChanges();   

        _logger.LogInformation("Seeded {total} clients.", Config.Clients.Count());
    }
}