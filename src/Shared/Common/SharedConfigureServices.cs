using Serilog;
using Serilog.Sinks.Elasticsearch;
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Shared.Common;

public static class SharedConfigureServices
{
    public static IServiceCollection AddSwaggerGeneration(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Name = "Authorization",
                Description = "Bearer Authentication with JWT Token",
                Type = SecuritySchemeType.Http
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Id = "Bearer",
                            Type = ReferenceType.SecurityScheme
                        }
                    },
                    new List<string>()
                }
            });
            
            options.OperationFilter<SwaggerDefaultValues>();
        });

        return services;
    }

    public static IServiceCollection AddApiVersioningConfiguration(this IServiceCollection services)
    {
        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1);
            options.ReportApiVersions = true;
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ApiVersionReader = new UrlSegmentApiVersionReader();
        })
        .AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'V";
            options.SubstituteApiVersionInUrl = true;
        });

        return services;
    }

    public static WebApplication UseSwaggerConfiguration(this WebApplication app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
            foreach (var description in provider.ApiVersionDescriptions)
            {
                options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant());
            }
        });

        return app;
    }

    public static IHostBuilder UseCustomSerilog(this IHostBuilder builder)
    {
        builder.UseSerilog((context, config) => 
        {
            var elasticSearchUri = context.Configuration["Serilog:ElasticsearchUri"] 
                ?? throw new ArgumentNullException("ElasticsearchUri configuration is required.");

            var environment = context.HostingEnvironment.EnvironmentName.Substring(0, 3).ToLower();
                
            config
                .Enrich.FromLogContext()
                .Enrich.WithProcessId()
                .Enrich.WithThreadId()
                .WriteTo.Console()
                .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(elasticSearchUri))
                {
                    // Custom index format (daily indices)
                    IndexFormat = $"{context.Configuration["Serilog:ApplicationName"]}-{environment}-logs-{DateTime.UtcNow:yyyy.MM.dd}",

                    // Automatically detect Elasticsearch clusters nodes
                    AutoRegisterTemplate = true,
                    AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv7,

                    NumberOfShards = 2,
                    NumberOfReplicas = 1,

                    // Connection configuration
                    ConnectionTimeout = TimeSpan.FromSeconds(5),
                    EmitEventFailure = EmitEventFailureHandling.WriteToSelfLog,

                    // Handle failures
                    FailureCallback = (@event) => Console.WriteLine($"Failed to send log: {@event.Exception?.Message}", @event)
                })
                .ReadFrom.Configuration(context.Configuration);

        });

        return builder;
    }
}