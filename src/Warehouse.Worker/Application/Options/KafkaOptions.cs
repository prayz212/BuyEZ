using Shared.Options;

using System.ComponentModel.DataAnnotations;

namespace WarehouseWorker.Application.Options;

public class KafkaOptions
{
    public const string SectionName = "Kafka";

    // TODO: using Fluent Validation instead (limit: not validate nested options)
    [Required(ErrorMessage = "Bootstrap server configuration is required.")]
    public required string BootstrapServer { get; set; }

    [Required(ErrorMessage = "Producers configuration is required.")]
    public required WarehouseProducer Producers { get; set; }

    [Required(ErrorMessage = "Consumers configuration is required.")]
    public required WarehouseConsumer Consumers { get; set; }
}

public class WarehouseProducer
{
    [Required(ErrorMessage = "OrderPackedEvent configuration is required.")]
    public required string OrderPackedEvent { get; set; }
}

public class WarehouseConsumer
{
    [Required(ErrorMessage = "OrderPlacedEvent configuration is required.")]
    public required EventOptions OrderPlacedEvent { get; set; }
}