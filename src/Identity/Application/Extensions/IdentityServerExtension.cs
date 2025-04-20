using Identity.Application.Domain;
using Identity.Application.Infrastructure.Persistence;

using Shared.Options;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Duende.IdentityServer.EntityFramework.DbContexts;

namespace Identity.Application.Extensions;

public static class IdentityServerExtension
{
    public static IServiceCollection AddCustomIdentityServer(this IServiceCollection services, IConfiguration configuration)
    {
        var identityOptions = configuration.GetSection(nameof(IdentityOptions));
        if (string.IsNullOrWhiteSpace(identityOptions["IssuerUri"]))
            throw new Exception("IdentityOptions:IssuerUri is a required configuration.");

        var identityServerBuilder = services.AddIdentityServer(options =>
            {
                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseInformationEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseSuccessEvents = true;
                options.IssuerUri = identityOptions["IssuerUri"];
            })
            .AddAspNetIdentity<User>()
            .AddConfigurationStore(options => 
                options.ConfigureDbContext = b => 
                    b.UseNpgsql(configuration.GetConnectionString("DefaultConnection"), 
                    b => b.MigrationsAssembly(typeof(ConfigurationDbContext).Assembly.FullName)))
            .AddOperationalStore(options => 
                options.ConfigureDbContext = b => 
                    b.UseNpgsql(configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly(typeof(PersistedGrantDbContext).Assembly.FullName))
                )
            .AddProfileService<ProfileService>()
            .AddResourceOwnerValidator<CustomResourceOwnerPassword>();

        //ref: https://documentation.openiddict.com/configuration/encryption-and-signing-credentials.html
        identityServerBuilder.AddDeveloperSigningCredential();

        return services;
    }
}