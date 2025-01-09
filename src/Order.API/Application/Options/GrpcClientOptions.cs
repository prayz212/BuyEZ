using Shared.Options;
using System.ComponentModel.DataAnnotations;

namespace OrderAPI.Application.Options;

public class GrpcClientOptions 
{
    // TODO: using Fluent Validation instead (limit: not validate nested options)
    [Required(ErrorMessage = "Catalog configuration is required for gRPC operations.")]
    public required GrpcBaseOptions Catalog { get; set; }
}