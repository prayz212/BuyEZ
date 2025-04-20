namespace Shared.Infrastructure.Persistence;

public interface IDbContextInitializer
{
    Task InitializeAsync();

    Task SeedAsync();
}