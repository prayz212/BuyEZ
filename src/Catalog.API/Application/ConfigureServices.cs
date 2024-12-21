using System.Reflection;
using CatalogAPI.Application.Common.Behaviors;
using CatalogAPI.Application.Common.Constants;
using CatalogAPI.Application.Common.Interfaces;
using CatalogAPI.Application.Infrastructure.Persistence;
using CatalogAPI.Application.Infrastructure.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

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

        var issuer = configuration["IdentityServer:Issuer"];
        if (string.IsNullOrWhiteSpace(issuer))
            throw new Exception("IdentityServer:Issuer is a required configuration.");

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options => 
            {
                options.Authority = issuer;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = issuer, 

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

        services.AddScoped<IDomainEventService, DomainEventService>();
        services.AddScoped<ApplicationDbContextInitializer>();

        return services;
    }
}