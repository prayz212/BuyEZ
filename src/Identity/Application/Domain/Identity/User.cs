using Identity.Application.Features.Account;
using Microsoft.AspNetCore.Identity;

namespace Identity.Application.Domain.Identity;

public class User : IdentityUser<Guid>
{
    public required string FirstName { get; init; }

    public required string LastName { get; init; }

    public string? TenantId { get; set; }

    /* Audit purposes */
    public DateTimeOffset Created { get; set; } = DateTimeOffset.UtcNow;

    public string? CreatedBy { get; set; }

    public DateTimeOffset? LastModified { get; set; }

    public string? LastModifiedBy { get; set; }

    public static IdentityAccountDetailResponse ToDto(User user) => 
        new (
            user.TenantId,
            user.Id.ToString(),
            user.FirstName,
            user.LastName,
            user.UserName!,
            user.Email!
        );
}