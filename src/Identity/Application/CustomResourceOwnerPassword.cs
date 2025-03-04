using Identity.Application.Domain;

using IdentityConstants = Shared.Common.Constants.IdentityConstants;

using Microsoft.AspNetCore.Identity;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Validation;
using Microsoft.Extensions.Logging;

namespace Identity.Application;

public class CustomResourceOwnerPassword(
    ILogger<CustomResourceOwnerPassword> logger, 
    SignInManager<User> signInManager, 
    UserManager<User> userManager
) : IResourceOwnerPasswordValidator
{
    private readonly ILogger<CustomResourceOwnerPassword> _logger = logger;
    private readonly SignInManager<User> _signInManager = signInManager;
    private readonly UserManager<User> _userManager = userManager;
    
    public async Task ValidateAsync(ResourceOwnerPasswordValidationContext context)
    {
        _logger.LogInformation("Handling validation for user {UserName}", context.UserName);

        var user = await _userManager.FindByNameAsync(context.UserName);
        if (user == null) 
        {
            context.Result = new GrantValidationResult(
                TokenRequestErrors.InvalidClient, "Invalid client.");
            return;
        }

        var userRoles = await _userManager.GetRolesAsync(user);
        _logger.LogInformation("User {UserName} has role(s): {@UserRoles}", context.UserName, userRoles);

        if (userRoles == null || !userRoles.Any() || userRoles.First() != IdentityConstants.Role.USER)
        {
            context.Result = new GrantValidationResult(
                TokenRequestErrors.InvalidRequest, "Invalid request.");
            return;
        }

        var signInResult = await _signInManager.PasswordSignInAsync(
            user, 
            context.Password, 
            isPersistent: true, 
            lockoutOnFailure: true);
            
        if (signInResult == null || !signInResult.Succeeded)
        {
            context.Result = new GrantValidationResult(
                TokenRequestErrors.UnauthorizedClient, "Invalid credentials.");
            return;
        }

        context.Result = new GrantValidationResult(
            subject: user.Id.ToString(),
            authenticationMethod: "custom",
            claims: [
                new("name", context.UserName),
                new("role", IdentityConstants.Role.USER),
                new("email", user.Email!),
            ]
        );
    }
}