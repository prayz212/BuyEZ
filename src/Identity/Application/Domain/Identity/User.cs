using Microsoft.AspNetCore.Identity;

namespace Identity.Application.Domain.Identity;

public class User : IdentityUser<Guid>
{
    public required string FirstName { get; init; }

    public required string LastName { get; init; }

    public string? TenantId { get; set; }
}