using Shared.Options;

using System.ComponentModel.DataAnnotations;

namespace CatalogAPI.Application.Options;

public class KafkaOptions
{
    public const string SectionName = "Kafka";
    
    // TODO: using Fluent Validation instead (limit: not validate nested options)
    [Required(ErrorMessage = "Bootstrap server configuration is required.")]
    public required string BootstrapServer { get; set; }

    [Required(ErrorMessage = "Product Created event configuration is required.")]
    public required EventOptions ProductCreatedEvent { get; set; }
}