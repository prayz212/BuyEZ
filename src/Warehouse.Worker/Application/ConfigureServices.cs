using WarehouseWorker.Application.Options;
using WarehouseWorker.Application.Features.Consumers;
using WarehouseWorker.Application.Infrastructure.Persistence;
using WarehouseWorker.Application.Domain.Interfaces.Repositories;
using WarehouseWorker.Application.Infrastructure.Persistence.Repositories;

using Shared.Common.Behaviors;
using Shared.Common.Interfaces;
using Shared.IntegrationEvents;
using Shared.Infrastructure.Services;

using Quartz;
using MassTransit;
using FluentValidation;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace WarehouseWorker.Application;

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
        // Add IOptions configurations
        services.AddOptions<KafkaOptions>()
            .BindConfiguration(KafkaOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        var connectionString = configuration.GetConnectionString("DefaultConnection");
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(
                connectionString,
                b => b.MigrationsHistoryTable("__WarehouseMigrationsHistory")));

        services.AddDbContext<BackgroundJobDbContext>(options =>
            options.UseNpgsql(
                connectionString,
                b => b.MigrationsHistoryTable("__BackgroundJobMigrationsHistory")));

        services.AddQuartz();
        services.AddQuartzHostedService(configure =>
        {
            configure.WaitForJobsToComplete = true;
        });

        services.AddMassTransit(config =>
            config.ConfigureMassTransit(services.BuildServiceProvider()));

        services.AddScoped<IDomainEventService, DomainEventService>();
        services.AddScoped<IPackageRepository, PackageRepository>();
        services.AddScoped<IJobHistoryRepository, JobHistoryRepository>();
        services.AddScoped<ApplicationDbContextInitializer>();

        return services;
    }
    
    public static void ConfigureMassTransit(this IBusRegistrationConfigurator config, IServiceProvider provider)
    {
        var options = provider.GetRequiredService<IOptions<KafkaOptions>>().Value;

        // TODO: using product broker instead, such as: Azure Service Bus, RabbitMQ
        config.UsingInMemory();

        config.AddRider(rider =>
        {
            rider.AddProducer<OrderPackedIntegrationEvent>(options.Producers.OrderPackedEvent);

            rider.AddConsumer<OrderPlacedConsumer>();

            rider.UsingKafka((context, configurator) =>
            {
                configurator.Host(options.BootstrapServer);

                var consumers = options.Consumers;
                configurator.TopicEndpoint<OrderPlacedIntegrationEvent>(
                    consumers.OrderPlacedEvent.Topic,
                    consumers.OrderPlacedEvent.GroupId,
                    e =>
                    {
                        e.ConfigureConsumer<OrderPlacedConsumer>(context);
                    });
            });
        });
    }
}