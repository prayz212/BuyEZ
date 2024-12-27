using System.ComponentModel.DataAnnotations;

namespace Shared.Options;

public class IdentityOptions
{
    // TODO: using Fluent Validation instead (limit: not validate nested options)
    [Required(ErrorMessage = "IssuerUri configuration is required.")]
    public required string IssuerUri { get; set; }
}