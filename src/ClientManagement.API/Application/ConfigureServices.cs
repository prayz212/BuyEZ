using System.Reflection;
using ClientManagementAPI.Application.Common.Behaviors;
using ClientManagementAPI.Application.Common.Interfaces;
using ClientManagementAPI.Application.Infrastructure.Persistence;
using ClientManagementAPI.Application.Infrastructure.Services;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ClientManagementAPI.Application;

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

        return services;
    }
}