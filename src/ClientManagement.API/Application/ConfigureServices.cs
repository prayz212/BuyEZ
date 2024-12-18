using System.Reflection;
using ClientManagementAPI.Application.Common.Behaviors;
using ClientManagementAPI.Application.Common.Constants;
using ClientManagementAPI.Application.Common.Interfaces;
using ClientManagementAPI.Application.Infrastructure.Persistence;
using ClientManagementAPI.Application.Infrastructure.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

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
            options.AddPolicy(nameof(PolicyConstants.SYSTEM_ADMIN_POLICY), policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireClaim("scope", IdentityConstants.StandardScopes.CLIENT_MANAGEMENT_API);
                policy.RequireRole(IdentityConstants.Role.SYSTEM_ADMIN);
            });
            
            options.AddPolicy(nameof(PolicyConstants.SYSTEM_SUPPORTER_POLICY), policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireClaim("scope", IdentityConstants.StandardScopes.CLIENT_MANAGEMENT_API);
                policy.RequireRole(IdentityConstants.Role.SYSTEM_SUPPORT);
            });

            options.AddPolicy(PolicyConstants.SYSTEM_ADMIN_OR_SUPPORTER_POLICY, policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireClaim("scope", IdentityConstants.StandardScopes.CLIENT_MANAGEMENT_API);
                policy.RequireRole(IdentityConstants.Role.SYSTEM_ADMIN, IdentityConstants.Role.SYSTEM_SUPPORT);
            });
        });

        services.AddScoped<IDomainEventService, DomainEventService>();

        return services;
    }
}