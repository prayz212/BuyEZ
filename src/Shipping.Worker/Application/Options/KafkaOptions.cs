using Shared.Options;

using System.ComponentModel.DataAnnotations;

namespace ShippingWorker.Application.Options;

public class KafkaOptions
{
    public const string SectionName = "Kafka";

    // TODO: using Fluent Validation instead (limit: not validate nested options)
    [Required(ErrorMessage = "Bootstrap server configuration is required.")]
    public required string BootstrapServer { get; set; }

    [Required(ErrorMessage = "Producers configuration is required.")]
    public required ShippingProducer Producers { get; set; }

    [Required(ErrorMessage = "Consumers configuration is required.")]
    public required ShippingConsumer Consumers { get; set; }
}

public class ShippingProducer
{
    [Required(ErrorMessage = "DeliveryStartedEvent configuration is required.")]
    public required string DeliveryStartedEvent { get; set; }

    [Required(ErrorMessage = "DeliverySucceededEvent configuration is required.")]
    public required string DeliverySucceededEvent { get; set; }
}

public class ShippingConsumer
{
    [Required(ErrorMessage = "OrderPackedEvent configuration is required.")]
    public required EventOptions OrderPackedEvent { get; set; }
}