using System.ComponentModel.DataAnnotations;

namespace Shared.Options;

public class GrpcOptions
{
    // TODO: using Fluent Validation instead (limit: not validate nested options)
    [Required(ErrorMessage = "API Key is required for gRPC operations.")]
    public string ApiKey { get; set; } = string.Empty;

    [Url(ErrorMessage = "Address is not supported for gRPC operations.")]
    public string? Address { get; set; }
    
    public List<string>? AllowedIPs { get; set; }
}