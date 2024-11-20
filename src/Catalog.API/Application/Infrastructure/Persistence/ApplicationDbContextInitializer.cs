using System.Text.Json;
using CatalogAPI.Application.Domain.Catalogs;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CatalogAPI.Application.Infrastructure.Persistence;

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
        if (_context.Products.Any())
        {
            _logger.LogInformation("The data already exists.");
            return;
        }

        var contentRootPath = $@"{Directory.GetParent(Environment.CurrentDirectory)?.Parent?.FullName}\Catalog.API\Application\Infrastructure";
        string sourcePath = Path.Combine(contentRootPath, "Seeds", "products.json");
        string sourceJson = File.ReadAllText(sourcePath);
        Product[]? sourceProductItems = JsonSerializer.Deserialize<Product[]>(sourceJson);

        if (sourceProductItems is null || sourceProductItems.Length == 0)
        {
            _logger.LogError("Cannot read product items from json file.");
            return;
        }

        // Seed product data
        _logger.LogInformation("Seeding product data...");
        await _context.Products.AddRangeAsync(sourceProductItems);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Added {total} product record(s)", sourceProductItems.Count());

        // Seed product image data
        sourcePath = Path.Combine(contentRootPath, "Seeds", "images.json");
        sourceJson = File.ReadAllText(sourcePath);
        Image[]? sourceImageItems = JsonSerializer.Deserialize<Image[]>(sourceJson);

        if (sourceImageItems is null || sourceImageItems.Length == 0)
        {
            _logger.LogError("Cannot read image items from json file.");
            return;
        }   

        _context.Images.RemoveRange(_context.Images);

        _logger.LogInformation("Seeding image data...");
        await _context.Images.AddRangeAsync(sourceImageItems);
        _ = await _context.SaveChangesAsync();
        _logger.LogInformation("Added {total} image records", sourceImageItems.Count());
    }
}
