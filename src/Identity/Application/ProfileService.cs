using Identity.Application.Domain;
using Identity.Application.Common;

using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using ClaimTypes = System.Security.Claims.ClaimTypes;
using Duende.IdentityServer.Extensions;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using static Duende.IdentityServer.IdentityServerConstants;

namespace Identity.Application;

public class ProfileService(UserManager<User> userManager) : IProfileService
{
    private readonly UserManager<User> _userManager = userManager;

    public async Task GetProfileDataAsync(ProfileDataRequestContext context)
    {
        /* Handle get profile data for user info endpoint */
        if (context.Caller == ProfileDataCallers.UserInfoEndpoint)
        {
            await HandleUserInfoEndpoint(context);
            return;
        }

        /* Validate user role based on ClientId */
        var userRole = context.Subject.FindFirstValue("role");
        ValidateUserRoleForClient(userRole, context.Client.ClientId);

        var userId = context.Subject.GetSubjectId();
        var user = (await _userManager.FindByIdAsync(userId))!;

        List<Claim> claims = [new(ClaimTypes.NameIdentifier, context.Subject.FindFirstValue("name")!)];

        /* Add tenant id if existed */
        if (!string.IsNullOrWhiteSpace(user.TenantId))
        {
            claims.Add(new("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/tenantid", user.TenantId));
        }

        /* Add claims to id token and user info endpoint */
        if (context.Caller == ProfileDataCallers.ClaimsProviderIdentityToken || 
            context.Caller == ProfileDataCallers.UserInfoEndpoint)
        {
            claims.Add(new(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"));
            claims.Add(new(ClaimTypes.Email, context.Subject.FindFirstValue("email")!));
        }

        /* Add claims to access token and user info endpoint */
        // TODO: Use Redis to store user information instead
        if (context.Caller == ProfileDataCallers.ClaimsProviderAccessToken || 
            context.Caller == ProfileDataCallers.UserInfoEndpoint)
        {
            claims.Add(new(ClaimTypes.Role, userRole!));
        }

        context.IssuedClaims.AddRange(claims);
    }

    public Task IsActiveAsync(IsActiveContext context)
    {
        // TODO: Check if the user is active or not
        context.IsActive = true;
        return Task.CompletedTask;
    }

    private void ValidateUserRoleForClient(string? userRole, string clientId)
    {
        if (string.IsNullOrWhiteSpace(userRole) || 
            !Config.IsInClientRole(clientId, userRole))
            throw new UnauthorizedAccessException($"User role {userRole} is not accessible to this client.");
    }

    private async Task HandleUserInfoEndpoint(ProfileDataRequestContext context)
    {
        var userId = context.Subject.GetSubjectId();
        var user = (await _userManager.FindByIdAsync(userId))!;
        var userRoles = await _userManager.GetRolesAsync(user);

        List<Claim> claims = [
            new(ClaimTypes.NameIdentifier, user.UserName!),
            new(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
            new(ClaimTypes.Email, user.Email!),
            new(ClaimTypes.Role, userRoles.First()),
        ];

        /* Add tenant id if existed */
        if (!string.IsNullOrWhiteSpace(user.TenantId))
        {
            claims.Add(new("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/tenantid", user.TenantId));
        }

        context.IssuedClaims.AddRange(claims);
    }
}