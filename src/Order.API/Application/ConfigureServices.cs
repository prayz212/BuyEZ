using OrderAPI.Application.Options;
using OrderAPI.Application.Features.Consumers;
using OrderAPI.Application.Infrastructure.Persistence;
using OrderAPI.Application.Domain.Interfaces.Repositories;
using OrderAPI.Application.Infrastructure.Persistence.Repositories;

using Shared.Options;
using Shared.GrpcProto.Catalog;
using Shared.Common.Constants;
using Shared.Common.Behaviors;
using Shared.Common.Interfaces;
using Shared.Common.Publishers;
using Shared.IntegrationEvents;
using Shared.Infrastructure.Services;

using MediatR;
using MassTransit;
using Grpc.Net.Client;
using ProtoBuf.Grpc.Client;
using System.Reflection;
using FluentValidation;
using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
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

        services.AddSingleton<INotificationPublisher, FaultTolerantNotificationPublisher>();
        services.AddMediatR(options =>
        {
            options.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());

            options.AddOpenBehavior(typeof(ValidationBehavior<,>));
            options.AddOpenBehavior(typeof(PerformanceBehavior<,>));
            options.AddOpenBehavior(typeof(UnhandledExceptionBehavior<,>));

            options.NotificationPublisher = services
                .BuildServiceProvider()
                .GetRequiredService<INotificationPublisher>();
        });

        services.AddOptions<GrpcClientOptions>()
            .Bind(configuration.GetSection(nameof(GrpcClientOptions)))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return services;
    }

    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Add IOptions configurations
        services.AddOptions<KafkaOptions>()
            .BindConfiguration(KafkaOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

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

                // TODO: using SSL certificate in real production and remove this workaround
                options.BackchannelHttpHandler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
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

        services.AddMassTransit(config =>
            config.ConfigureMassTransit(services.BuildServiceProvider()));

        services.AddScoped<IDomainEventService, DomainEventService>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<ApplicationDbContextInitializer>();
        services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();

        return services;
    }

    public static void ConfigureMassTransit(this IBusRegistrationConfigurator config, IServiceProvider provider)
    {
        var options = provider.GetRequiredService<IOptions<KafkaOptions>>().Value;

        // TODO: using product broker instead, such as: Azure Service Bus, RabbitMQ
        config.UsingInMemory();

        config.AddRider(rider =>
        {
            var producers = options.Producers;
            rider.AddProducer<OrderCreatedIntegrationEvent>(producers.OrderCreatedEvent);

            var consumers = options.Consumers;
            rider.AddConsumer<OrderPackingStartedConsumer>();
            rider.AddConsumer<DeliveryStartedConsumer>();
            rider.AddConsumer<DeliverySucceededConsumer>();

            rider.UsingKafka((context, configurator) =>
            {
                configurator.Host(options.BootstrapServer);

                configurator.TopicEndpoint<OrderPackingStartedIntegrationEvent>(
                    consumers.OrderPackingStartedEvent.Topic,
                    consumers.OrderPackingStartedEvent.GroupId,
                    e => e.ConfigureConsumer<OrderPackingStartedConsumer>(context));

                configurator.TopicEndpoint<DeliveryStartedIntegrationEvent>(
                    consumers.DeliveryStartedEvent.Topic,
                    consumers.DeliveryStartedEvent.GroupId,
                    e => e.ConfigureConsumer<DeliveryStartedConsumer>(context));

                configurator.TopicEndpoint<DeliverySucceededIntegrationEvent>(
                    consumers.DeliverySucceededEvent.Topic,
                    consumers.DeliverySucceededEvent.GroupId,
                    e => e.ConfigureConsumer<DeliverySucceededConsumer>(context));
            });
        });
    }
}