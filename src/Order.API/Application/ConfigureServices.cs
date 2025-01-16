using OrderAPI.Application.Options;
using OrderAPI.Application.Infrastructure.Persistence;

using Shared.Options;
using Shared.GrpcProto.Catalog;
using Shared.Common.Constants;
using Shared.Common.Behaviors;
using Shared.Common.Interfaces;
using Shared.Infrastructure.Services;

using Grpc.Net.Client;
using ProtoBuf.Grpc.Client;
using System.Reflection;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace OrderAPI.Application;

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
        var catalogAddress = grpcClientOptions["Catalog:Address"];
        if (string.IsNullOrWhiteSpace(catalogAddress))
            throw new Exception("Catalog:Address is a required configuration.");

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
            options.AddPolicy(PolicyConstants.CUSTOMER_POLICY, policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireClaim("scope", IdentityConstants.StandardScopes.ORDER_API);
                policy.RequireRole(IdentityConstants.Role.USER);
            });
        });

        services.AddSingleton(provider => 
        {
            // TODO: using SSL certificate in real production and remove this workaround
            var httpHandler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };
            
            var channel = GrpcChannel.ForAddress(catalogAddress, new GrpcChannelOptions
            {
                HttpClient = new(httpHandler)
            });
            return channel.CreateGrpcService<ICatalogService>();
        });

        services.AddScoped<IDomainEventService, DomainEventService>();
        services.AddScoped<ApplicationDbContextInitializer>();

        return services;
    }
}