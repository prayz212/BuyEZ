using Asp.Versioning.ApiExplorer;
using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Reflection;
using Microsoft.OpenApi.Models;

namespace Shared.Options;

public class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
{
    private readonly IApiVersionDescriptionProvider _provider;

    public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider)
    {
        _provider = provider;
    }

    public void Configure(SwaggerGenOptions options)
    {
        var moduleName = Assembly.GetEntryAssembly()?.Modules.FirstOrDefault()?.Name;
        foreach (var description in _provider.ApiVersionDescriptions)
        {
            options.SwaggerDoc(description.GroupName, CreateInfoForApiVersion(description, moduleName ?? string.Empty));
        }
    }

    private static OpenApiInfo CreateInfoForApiVersion(ApiVersionDescription description, string module)
    {
        var info = GetModuleInfo(module);
        info.Version = description.ApiVersion.ToString();

        if (description.IsDeprecated)
        {
            info.Description = $"[DEPRECATED VERSION] {info.Description}";
        }

        return info;
    }

    private static OpenApiInfo GetModuleInfo(string module) => module switch
    {
        "Catalog.API.dll" => new() 
        {
            Title = "Catalog Management APIs",
            Description = "This API allows clients to interact with the product catalog, enabling them to create, read, update, and delete products, categories.",   
        },

        "ClientManagement.API.dll" => new()
        {
            Title = "Client Management APIs",
            Description = "This API allows clients to interact with the tenant clients, enabling them to create, read, update, and delete client.",   
        },

        "Identity.API.dll" => new()
        {
            Title = "Identity Management APIs",
            Description = "This API allows clients to interact with the user account, enabling them to create, read, update, and delete account.",
        },

        "Order.API.dll" => new()
        {
            Title = "Order Management APIs",
            Description = "This API allows clients to interact with the order, enabling them to create, read, update, and delete order.",
        },
         
        _ => throw new Exception("Unknown module")
    };
}