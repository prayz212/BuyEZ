using System.ComponentModel.DataAnnotations;

namespace Shared.Options;

public class GrpcServerOptions
{
    // TODO: using Fluent Validation instead (limit: not validate nested options)
    [Required(ErrorMessage = "API Key - Service pair is required for gRPC operations.")]
    public Dictionary<string, string[]> ApiKeys { get; set; } = [];

    public List<string>? AllowedIPs { get; set; }
}