using System.ComponentModel.DataAnnotations;

namespace Identity.Application.Infrastructure.Options;

public class ServiceOptions
{
    // TODO: using Fluent Validation instead (limit: not validate nested options)
    [Required(ErrorMessage = "Service options configuration is required.")]
    public required string BaseUrl { get; set; }
}