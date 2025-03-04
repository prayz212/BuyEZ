using ClientManagementAPI.Application.Domain;

using Shared.Common;
using Shared.Common.Interfaces;

using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClientManagementAPI.Application.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    private readonly ILogger<ApplicationDbContext> _logger;
    private readonly IDomainEventService _domainEventService;

    public DbSet<Client> Clients => Set<Client>();
    public DbSet<Image> Images => Set<Image>(); 

    public ApplicationDbContext(ILogger<ApplicationDbContext> logger, DbContextOptions<ApplicationDbContext> options, IDomainEventService domainEventService) : base(options)
    {
        _logger = logger;
        _domainEventService = domainEventService;
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    // TODO: implement later
                    // entry.Entity.CreatedBy = "system"; 
                    entry.Entity.Created = DateTimeOffset.UtcNow;
                    break;
                case EntityState.Modified:
                    // TODO: implement later
                    // entry.Entity.LastModifiedBy = "system";       
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