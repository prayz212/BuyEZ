using Identity.Application.Common;
using Identity.Application.Common.Entities;
using Identity.Application.Common.Models;
using Identity.Application.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Identity.Application.Extensions;

public static class IdentityServerExtension
{
    public static IServiceCollection AddCustomIdentityServer(this IServiceCollection services, IConfiguration configuration)
    {
        var authOptions = configuration.GetSection(nameof(AuthOptions));
        if (string.IsNullOrWhiteSpace(authOptions["IssuerUri"]))
            throw new Exception("AuthOptions:IssuerUri is a required configuration.");

        var connectionString = configuration.GetConnectionString("DefaultConnection");
        
        services.AddIdentity<User, IdentityRole<Guid>>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        var identityServerBuilder = services.AddIdentityServer(options =>
            {
                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseInformationEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseSuccessEvents = true;
                options.IssuerUri = authOptions["IssuerUri"];
            })
            .AddInMemoryIdentityResources(Config.IdentityResources)
            .AddInMemoryApiResources(Config.ApiResources)
            .AddInMemoryApiScopes(Config.ApiScopes)
            .AddInMemoryClients(Config.Clients)
            .AddAspNetIdentity<User>();

        //ref: https://documentation.openiddict.com/configuration/encryption-and-signing-credentials.html
        identityServerBuilder.AddDeveloperSigningCredential();

        return services;
    }
}