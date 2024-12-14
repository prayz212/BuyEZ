using Microsoft.AspNetCore.Identity;

namespace Identity.Application.Common.Entities;

public class User : IdentityUser<Guid>
{
    public required string FirstName { get; init; }

    public required string LastName { get; init; }
}