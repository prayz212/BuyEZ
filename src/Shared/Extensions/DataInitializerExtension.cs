using Shared.Infrastructure.Persistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Shared.Extensions;

public static class DataInitializerExtension
{
    public static async Task InitializeDatabaseAsync<TInitializer>(this WebApplication app) where TInitializer : IDbContextInitializer
    {
        using IServiceScope scope = app.Services.CreateScope();
        
        IDbContextInitializer initializer = scope.ServiceProvider.GetRequiredService<TInitializer>();

        await initializer.InitializeAsync();

        await initializer.SeedAsync();
    }
}