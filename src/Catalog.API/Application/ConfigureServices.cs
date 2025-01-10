using CatalogAPI.Application.Features.Shopping.gRPC;
using CatalogAPI.Application.Infrastructure.Persistence;

using Shared.Options;
using Shared.GrpcProto;
using Shared.Common.Behaviors;
using Shared.Common.Constants;
using Shared.Common.Interfaces;
using Shared.Infrastructure.Services;

using System.Reflection;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Builder;
using ProtoBuf.Grpc.Server;

namespace CatalogAPI.Application;

public static class DependencyInjection {
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
                    // IssuerSigningKeyResolver = (token, securityToken, kid, parameters) =>
                    // {
                    //     // Optional: Custom logic to resolve signing keys if needed
                    //     return parameters.IssuerSigningKeys;
                    // }
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

        services.AddScoped<IDomainEventService, DomainEventService>();
        services.AddScoped<ApplicationDbContextInitializer>();

        return services;
    }

    public static WebApplication MapGrpcServices(this WebApplication app)
    {
        app.MapGrpcService<CatalogService>();

        return app;
    }
}