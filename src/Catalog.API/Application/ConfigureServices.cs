using System.Reflection;
using CatalogAPI.Application.Common.Behaviors;
using CatalogAPI.Application.Common.Interfaces;
using CatalogAPI.Application.Infrastructure.Persistence;
using CatalogAPI.Application.Infrastructure.Services;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CatalogAPI.Application;

public static class DependencyInjection {
    public static IServiceCollection AddApplication(this IServiceCollection services) 
    {
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        services.AddMediatR(options => 
        {
            options.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());

            options.AddOpenBehavior(typeof(ValidationBehavior<,>));
            options.AddOpenBehavior(typeof(PerformanceBehavior<,>));
            options.AddOpenBehavior(typeof(UnhandledExceptionBehavior<,>));
        });

        return services;
    }

    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration) 
    {
        services.AddDbContext<ApplicationDbContext>(options => 
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        services.AddScoped<IDomainEventService, DomainEventService>();
        services.AddScoped<ApplicationDbContextInitializer>();

        return services;
    }
}