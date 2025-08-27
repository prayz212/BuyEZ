using CatalogAPI.Application.Options;
using CatalogAPI.Application.Features.Shopping.gRPC;
using CatalogAPI.Application.Infrastructure.Persistence;
using CatalogAPI.Application.Domain.Interfaces.Repositories;
using CatalogAPI.Application.Infrastructure.Persistence.Repositories;

using Shared.Options;
using Shared.GrpcProto;
using Shared.Common.Behaviors;
using Shared.Common.Constants;
using Shared.Common.Interfaces;
using Shared.IntegrationEvents;
using Shared.Infrastructure.Services;

using MassTransit;
using System.Reflection;
using FluentValidation;
using ProtoBuf.Grpc.Server;
using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Builder;

namespace CatalogAPI.Application;

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

        services.AddOptions<GrpcServerOptions>()
            .Bind(configuration.GetSection(nameof(GrpcServerOptions)))
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

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        var identityOptions = configuration.GetSection(nameof(IdentityOptions));
        if (string.IsNullOrWhiteSpace(identityOptions["IssuerUri"]))
            throw new Exception("IdentityOptions:IssuerUri is a required configuration.");

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options => 
            {
                options.Authority = identityOptions["IssuerUri"];
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = identityOptions["IssuerUri"], 

                    // TODO: need to investigate why we don't have audience in Identity Server's token
                    ValidateAudience = false,

                    ValidateLifetime = true,
                };

                // TODO: using SSL certificate in real production and remove this workaround
                options.BackchannelHttpHandler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                };
            });

        services.AddAuthorization(options =>
        {
            options.AddPolicy(PolicyConstants.TENANT_ADMIN_POLICY, policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireClaim("scope", IdentityConstants.StandardScopes.CATALOG_API);
                policy.RequireRole(IdentityConstants.Role.TENANT_ADMIN);
            });

            options.AddPolicy(PolicyConstants.TENANT_MANAGER_POLICY, policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireClaim("scope", IdentityConstants.StandardScopes.CATALOG_API);
                policy.RequireRole(IdentityConstants.Role.TENANT_MANAGER);
            });

            options.AddPolicy(PolicyConstants.TENANT_STAFF_POLICY, policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireClaim("scope", IdentityConstants.StandardScopes.CATALOG_API);
                policy.RequireRole(IdentityConstants.Role.TENANT_STAFF);
            });

            options.AddPolicy(PolicyConstants.TENANT_ADMIN_OR_MANAGER_POLICY, policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireClaim("scope", IdentityConstants.StandardScopes.CATALOG_API);
                policy.RequireRole(IdentityConstants.Role.TENANT_ADMIN, IdentityConstants.Role.TENANT_MANAGER);
            });

            options.AddPolicy(PolicyConstants.TENANT_ADMIN_OR_MANAGER_OR_STAFF_POLICY, policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireClaim("scope", IdentityConstants.StandardScopes.CATALOG_API);
                policy.RequireRole(IdentityConstants.Role.TENANT_ADMIN, IdentityConstants.Role.TENANT_MANAGER, IdentityConstants.Role.TENANT_STAFF);
            });
        });

        services.AddCodeFirstGrpc(options =>
        {
            options.Interceptors.Add<GrpcExceptionInterceptor>();
            options.Interceptors.Add<GrpcApiKeyInterceptor>();
            options.EnableDetailedErrors = true;
        });

        services.AddCodeFirstGrpcReflection();

        services.AddMassTransit(config =>
            config.ConfigureMassTransit(services.BuildServiceProvider()));

        services.AddScoped<IDomainEventService, DomainEventService>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<ApplicationDbContextInitializer>();

        services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();

        return services;
    }

    public static WebApplication MapGrpcServices(this WebApplication app)
    {
        app.MapGrpcService<CatalogService>();

        return app;
    }

    public static void ConfigureMassTransit(this IBusRegistrationConfigurator config, IServiceProvider provider)
    {
        var options = provider.GetRequiredService<IOptions<KafkaOptions>>().Value;

        // TODO: using product broker instead, such as: Azure Service Bus, RabbitMQ
        config.UsingInMemory();

        config.AddRider(rider =>
        {
            rider.AddProducer<ProductCreatedIntegrationEvent>(options.ProductCreatedEvent.Topic);

            rider.UsingKafka((context, configurator) =>
            {
                configurator.Host(options.BootstrapServer);
            });
        });
    }
}