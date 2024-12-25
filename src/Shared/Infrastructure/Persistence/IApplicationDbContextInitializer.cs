namespace Shared.Infrastructure.Persistence;

public interface IApplicationDbContextInitializer
{
    public Task InitializeAsync();

    public Task SeedAsync();
}