using CatalogAPI.Application.Domain;

using Shared.Common;
using Shared.Common.Interfaces;

using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CatalogAPI.Application.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    private readonly IDomainEventService _domainEventService;
    private readonly ILogger<ApplicationDbContext> _logger;

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Image> Images => Set<Image>();

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options, 
        IDomainEventService domainEventService, 
        ILogger<ApplicationDbContext> logger) : base(options)
    {
        _domainEventService = domainEventService;
        _logger = logger;
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.Created = DateTimeOffset.UtcNow;
                    break;
                case EntityState.Modified: 
                    entry.Entity.LastModified = DateTimeOffset.UtcNow;
                    break;
                case EntityState.Deleted:
                case EntityState.Detached:
                case EntityState.Unchanged:
                    break;
                default:
                    break;
            }
        }

        var events = ChangeTracker.Entries<IHasDomainEvent>()
            .Select(x => x.Entity.DomainEvents)
            .SelectMany(x => x)
            .Where(domainEvent => !domainEvent.IsPublished)
            .ToList();

        _logger.LogInformation("Saving changes to database...");
        var result = await base.SaveChangesAsync(cancellationToken);

        _logger.LogInformation($"Dispatching {events.Count} events...");
        await DispatchEvents(events);

        return result;
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(builder);
    }

    private async Task DispatchEvents(List<DomainEvent> events)
    {
        foreach (var @event in events)
        {
            @event.IsPublished = true;
            await _domainEventService.Publish(@event);
        }
    }
}