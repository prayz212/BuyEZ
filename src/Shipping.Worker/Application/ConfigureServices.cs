using ShippingWorker.Application.Options;
using ShippingWorker.Application.Infrastructure.Persistence;
using ShippingWorker.Application.Domain.Interfaces.Repositories;
using ShippingWorker.Application.Infrastructure.Persistence.Repositories;

using Shared.Common.Behaviors;
using Shared.Common.Interfaces;
using Shared.Infrastructure.Services;

using Quartz;
using FluentValidation;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ShippingWorker.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        
        services.AddMediatR(options => 
        {
            options.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());

            options.AddOpenBehavior(typeof(ValidationBehavior<,>));
            options.AddOpenBehavior(typeof(PerformanceBehavior<,>));
            options.AddOpenBehavior(typeof(UnhandledExceptionBehavior<,>));
        });

        services.AddOptions<JobCronOptions>()
            .Bind(configuration.GetSection(nameof(JobCronOptions)))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return services;
    }

    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        services.AddDbContext<ApplicationDbContext>(options => 
            options.UseNpgsql(
                connectionString,
                b => b.MigrationsHistoryTable("__ShipmentMigrationsHistory")));

        services.AddDbContext<BackgroundJobDbContext>(options => 
            options.UseNpgsql(
                connectionString,
                b => b.MigrationsHistoryTable("__BackgroundJobMigrationsHistory")));

        services.AddQuartz();
        services.AddQuartzHostedService(configure => 
        {
            configure.WaitForJobsToComplete = true;
        });

        services.AddScoped<IDomainEventService, DomainEventService>();
        services.AddScoped<IShipmentRepository, ShipmentRepository>();
        services.AddScoped<IJobHistoryRepository, JobHistoryRepository>();
        services.AddScoped<ApplicationDbContextInitializer>();

        return services;
    }
}