using Shared.Options;

using System.ComponentModel.DataAnnotations;

namespace OrderAPI.Application.Options;

public class KafkaOptions
{
    public const string SectionName = "Kafka";

    // TODO: using Fluent Validation instead (limit: not validate nested options)
    [Required(ErrorMessage = "Bootstrap server configuration is required.")]
    public required string BootstrapServer { get; set; }

    [Required(ErrorMessage = "Producers configuration is required.")]
    public required OrderProducer Producers { get; set; }

    [Required(ErrorMessage = "Consumers configuration is required.")]
    public required OrderConsumer Consumers { get; set; }
}

public class OrderProducer
{
    [Required(ErrorMessage = "OrderCreatedEvent configuration is required.")]
    public required string OrderCreatedEvent { get; set; }

    [Required(ErrorMessage = "OrderPlacedEvent configuration is required.")]
    public required string OrderPlacedEvent { get; set; }
}

public class OrderConsumer
{
    [Required(ErrorMessage = "ProductCreatedEvent configuration is required.")]
    public required EventOptions ProductCreatedEvent { get; set; }

    [Required(ErrorMessage = "OrderPackingStartedEvent configuration is required.")]
    public required EventOptions OrderPackingStartedEvent { get; set; }

    [Required(ErrorMessage = "DeliveryStartedEvent configuration is required.")]
    public required EventOptions DeliveryStartedEvent { get; set; }    
}