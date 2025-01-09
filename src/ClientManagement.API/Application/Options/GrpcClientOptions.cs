using Shared.Options;
using System.ComponentModel.DataAnnotations;

namespace ClientManagementAPI.Application.Options;

public class GrpcClientOptions 
{
    // TODO: using Fluent Validation instead (limit: not validate nested options)
    [Required(ErrorMessage = "Identity configuration is required for gRPC operations.")]
    public required GrpcBaseOptions Identity { get; set; }
}