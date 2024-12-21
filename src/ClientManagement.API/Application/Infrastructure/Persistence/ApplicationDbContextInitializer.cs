using System.Text.Json;
using ClientManagementAPI.Application.Domain.Clients;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ClientManagementAPI.Application.Infrastructure.Persistence;

public static class InitializerExtensions
{
    public static async Task InitializeDatabaseAsync(this WebApplication app)
    {
        using IServiceScope scope = app.Services.CreateScope();

        ApplicationDbContextInitializer initializer = scope.ServiceProvider.GetRequiredService<ApplicationDbContextInitializer>();

        await initializer.InitializeAsync();

        await initializer.SeedAsync();
    }
}

public class ApplicationDbContextInitializer
{
    private readonly ILogger<ApplicationDbContextInitializer> _logger;
    private readonly ApplicationDbContext _context;

    public ApplicationDbContextInitializer(
        ILogger<ApplicationDbContextInitializer> logger, 
        ApplicationDbContext context
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

    public async Task TrySeedAsync()
    {
        /* Seeding system users */
        if (await _context.Clients.AnyAsync())
        {
            _logger.LogInformation("Client already exists.");
            return;
        }

        var contentRootPath = $@"{Directory.GetParent(Environment.CurrentDirectory)?.Parent?.FullName}\ClientManagement.API\Application\Infrastructure";
        string sourcePath = Path.Combine(contentRootPath, "Seeds", "clients.json");
        string sourceJson = File.ReadAllText(sourcePath);
        var sourceItems = JsonSerializer.Deserialize<Client[]>(sourceJson);

        if (sourceItems is null || sourceItems.Length == 0)
        {
            _logger.LogError("Cannot read client items from json file.");
            return;
        }

        _logger.LogInformation("Seeding clients...");

        List<Client> clients = sourceItems.ToList();
        clients.ForEach(client => 
        {
            client.ValidUntil = DateTime.Now.AddYears(1);
        });

        await _context.AddRangeAsync(clients);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Added {total} client record(s)", clients.Count());
    }
}
