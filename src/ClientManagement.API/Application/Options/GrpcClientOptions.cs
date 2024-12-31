using Shared.Options;
using System.ComponentModel.DataAnnotations;

namespace ClientManagementAPI.Application.Options;

public class GrpcClientOptions 
{
    // TODO: using Fluent Validation instead (limit: not validate nested options)
    [Required(ErrorMessage = "Client Management configuration is required for gRPC operations.")]
    public required GrpcBaseOptions ClientManagement { get; set; }
}