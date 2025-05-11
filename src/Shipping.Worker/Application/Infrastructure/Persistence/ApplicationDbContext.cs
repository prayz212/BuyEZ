using ShippingWorker.Application.Domain;

using Shared.Common;
using Shared.Common.Interfaces;

using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ShippingWorker.Application.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    private readonly ILogger<ApplicationDbContext> _logger;
    private readonly IDomainEventService _domainEventService;

    public DbSet<Shipment> Shipments => Set<Shipment>();
    public DbSet<JobExecutionHistory> JobExecutionHistories => Set<JobExecutionHistory>();
    public DbSet<ShipmentTrackingEvent> ShipmentTrackingEvents => Set<ShipmentTrackingEvent>();

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options, 
        ILogger<ApplicationDbContext> logger, 
        IDomainEventService domainEventService) : base(options)
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