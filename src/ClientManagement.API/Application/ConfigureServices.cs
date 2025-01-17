using ClientManagementAPI.Application.Options;
using ClientManagementAPI.Application.Infrastructure.Persistence;

using Shared.Options;
using Shared.Common.Behaviors;
using Shared.Common.Constants;
using Shared.Common.Interfaces;
using Shared.GrpcProto.Account;
using Shared.Infrastructure.Services;

using Grpc.Net.Client;
using System.Reflection;
using FluentValidation;
using ProtoBuf.Grpc.Client;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace ClientManagementAPI.Application;

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

        services.AddOptions<GrpcClientOptions>()
            .Bind(configuration.GetSection(nameof(GrpcClientOptions)))
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

        var grpcClientOptions = configuration.GetSection(nameof(GrpcClientOptions));
        var identityAddress = grpcClientOptions["Identity:Address"];
        if (string.IsNullOrWhiteSpace(identityAddress))
            throw new Exception("Identity:Address is a required configuration.");

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
            options.AddPolicy(PolicyConstants.SYSTEM_ADMIN_POLICY, policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireClaim("scope", IdentityConstants.StandardScopes.CLIENT_MANAGEMENT_API);
                policy.RequireRole(IdentityConstants.Role.SYSTEM_ADMIN);
            });
            
            options.AddPolicy(PolicyConstants.SYSTEM_SUPPORTER_POLICY, policy =>
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

            options.AddPolicy(PolicyConstants.TENANT_ADMIN_OR_MANAGER_OR_STAFF_POLICY, policy =>
            {
               policy.RequireAuthenticatedUser();
               policy.RequireRole(IdentityConstants.Role.TENANT_ADMIN, IdentityConstants.Role.TENANT_MANAGER, IdentityConstants.Role.TENANT_STAFF);
            });
        });

        services.AddScoped<IDomainEventService, DomainEventService>();
        services.AddScoped<ApplicationDbContextInitializer>();

        services.AddSingleton(provider =>
        {
            var channel = GrpcChannel.ForAddress(identityAddress);
            return channel.CreateGrpcService<IAccountService>();
        });

        return services;
    }
}