using Identity.Application.Domain.Identity;
using Identity.Application.Features.Identity.Shared.RestAPIs;
using Identity.Application.Infrastructure.Persistence;

using Shared.Common.Behaviors;
using Shared.Common.Constants;
using IdentityConstants = Shared.Common.Constants.IdentityConstants;

using Refit;
using System.Reflection;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Identity.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        var identityOptions = configuration.GetSection(nameof(IdentityOptions));
        if (string.IsNullOrWhiteSpace(identityOptions["IssuerUri"]))
            throw new Exception("IdentityOptions:IssuerUri is a required configuration.");

        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        services.AddMediatR(options => 
        {
            options.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());

            options.AddOpenBehavior(typeof(ValidationBehavior<,>));
            options.AddOpenBehavior(typeof(PerformanceBehavior<,>));
            options.AddOpenBehavior(typeof(UnhandledExceptionBehavior<,>));
        });

        services.AddRefitClient<IIdentityServerApi>()
            .ConfigureHttpClient(config => 
            {
                config.BaseAddress = new Uri(identityOptions["IssuerUri"]!);
            });

        return services;
    }

    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
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
            options.AddPolicy(PolicyConstants.SYSTEM_ADMIN_POLICY, policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireClaim("scope", IdentityConstants.StandardScopes.IDENTITY_API);
                policy.RequireRole(IdentityConstants.Role.SYSTEM_ADMIN);
            });
            
            options.AddPolicy(PolicyConstants.SYSTEM_SUPPORTER_POLICY, policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireClaim("scope", IdentityConstants.StandardScopes.IDENTITY_API);
                policy.RequireRole(IdentityConstants.Role.SYSTEM_SUPPORT);
            });

            options.AddPolicy(PolicyConstants.SYSTEM_ADMIN_OR_SUPPORTER_POLICY, policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireClaim("scope", IdentityConstants.StandardScopes.IDENTITY_API);
                policy.RequireRole(IdentityConstants.Role.SYSTEM_ADMIN, IdentityConstants.Role.SYSTEM_SUPPORT);
            });
        });

        services.AddServices(configuration);
        services.AddScoped<ApplicationDbContextInitializer>();

        return services;
    }

    public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options => 
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)
            )
        );

        services.AddIdentity<User, IdentityRole<Guid>>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        return services;
    }
}