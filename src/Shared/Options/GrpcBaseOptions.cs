using System.ComponentModel.DataAnnotations;

namespace Shared.Options;

public class GrpcBaseOptions
{
    // TODO: using Fluent Validation instead (limit: not validate nested options)
    [Required(ErrorMessage = "API Key is required for gRPC operations.")]
    public string ApiKey { get; set; } = string.Empty;

    [Required(ErrorMessage = "Address is required for gRPC operations.")]
    public string Address { get; set; } = string.Empty;
}