using System.Reflection;
using ClientManagementAPI.Application.Common;
using ClientManagementAPI.Application.Common.Interfaces;
using ClientManagementAPI.Application.Domain.Clients;
using Microsoft.EntityFrameworkCore;

namespace ClientManagementAPI.Application.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    private readonly IDomainEventService _domainEventService;

    public DbSet<Client> Clients => Set<Client>();
    public DbSet<Image> Images => Set<Image>(); 

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IDomainEventService domainEventService) : base(options)
    {
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

        var result = await base.SaveChangesAsync(cancellationToken);

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