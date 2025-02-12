using Identity.Application.Domain;
using Identity.Application.Shared.RestAPIs;
using Identity.Application.Infrastructure.Persistence;
using Identity.Application.Features.Administration.gRPC;
using Identity.Application.Infrastructure.Options;

using Shared.Options;
using Shared.GrpcProto;
using Shared.Common.Behaviors;
using Shared.Common.Constants;
using IdentityOptions = Shared.Options.IdentityOptions;
using IdentityConstants = Shared.Common.Constants.IdentityConstants;

using Refit;
using FluentValidation;
using System.Reflection;
using ProtoBuf.Grpc.Server;
using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Builder;

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
            .ConfigureHttpClient(config => config.BaseAddress = new Uri(identityOptions["IssuerUri"]!))
            // TODO: using SSL certificate in real production and remove this workaround
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            });

        services.AddOptions<GrpcServerOptions>()
            .Bind(configuration.GetSection(nameof(GrpcServerOptions)))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        
        services.AddOptions<ServiceOptions>()
            .Bind(configuration.GetSection(nameof(ServiceOptions)))
            .ValidateDataAnnotations()
            .ValidateOnStart();
            
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

        services.AddCodeFirstGrpc(options => 
        {
            options.Interceptors.Add<GrpcExceptionInterceptor>();
            options.Interceptors.Add<GrpcApiKeyInterceptor>();
            options.EnableDetailedErrors = true;
        });

        services.AddCodeFirstGrpcReflection();

        services.AddServices(configuration);
        
        services.AddScoped<ApplicationDbContextInitializer>();
        services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();

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

        services.AddIdentity<User, IdentityRole<Guid>>(options =>
        {
            // Configure password reset token expiration
            options.Tokens.PasswordResetTokenProvider = TokenOptions.DefaultProvider;
        })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        services.Configure<DataProtectionTokenProviderOptions>(options =>
        {
            // Set expiration time for password reset token
            options.TokenLifespan = TimeSpan.FromMinutes(10);
        });

        services.Configure<ServiceOptions>(options =>
        {
            configuration.GetSection(nameof(ServiceOptions));
        });

        return services;
    }

    public static WebApplication MapGrpcServices(this WebApplication app)
    {
        app.MapGrpcService<AccountService>();

        return app;
    }
}