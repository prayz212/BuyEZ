using Newtonsoft.Json;
using Microsoft.AspNetCore.Identity;

namespace Identity.Application.Domain;

public class User : IdentityUser<Guid>
{
    [JsonProperty("firstName")]
    public string FirstName { get; set; } = string.Empty;

    [JsonProperty("lastName")]
    public string LastName { get; set; } = string.Empty;

    [JsonProperty("tenantId")]
    public string? TenantId { get; set; }

    /* Audit purposes */
    public DateTimeOffset Created { get; set; } = DateTimeOffset.UtcNow;

    public string? CreatedBy { get; set; }

    public DateTimeOffset? LastModified { get; set; }

    public string? LastModifiedBy { get; set; }

    // Constructors
    /*
        - By default, compiler will create a default parameterless constructor.
            => Compiler will NOT generate a parameterless constructor in case we already defined parameters constructor
     
        - Parameterless constructor for EF Core to instantiate the object with the data got from database
     */
    internal User() { }

    private User(
        string firstName,
        string lastName,
        string userName,
        string email,
        string phoneNumber,
        string tenantId,
        string createdBy)
    {
        Id = Guid.NewGuid();
        FirstName = firstName;
        LastName = lastName;
        UserName = userName;
        NormalizedUserName = userName.ToUpper();
        Email = email;
        NormalizedEmail = email.ToUpper();
        PhoneNumber = phoneNumber;
        TenantId = tenantId;
        CreatedBy = createdBy;
    }

    public static User CreateNew(
        string firstName,
        string lastName,
        string userName,
        string email,
        string? phoneNumber = default,
        string? tenantId = default,
        string? createdBy = default)
    {
        return new(
            firstName,
            lastName,
            userName,
            email,
            phoneNumber ?? string.Empty,
            tenantId ?? string.Empty,
            createdBy ?? string.Empty);
    }
}