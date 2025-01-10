using Identity.Application.Domain;

using IdentityConstants = Shared.Common.Constants.IdentityConstants;

using Microsoft.AspNetCore.Identity;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Validation;

namespace Identity.Application;

public class CustomResourceOwnerPassword(SignInManager<User> signInManager, UserManager<User> userManager) : IResourceOwnerPasswordValidator
{
    private readonly SignInManager<User> _signInManager = signInManager;
    private readonly UserManager<User> _userManager = userManager;
    
    public async Task ValidateAsync(ResourceOwnerPasswordValidationContext context)
    {
        var user = await _userManager.FindByNameAsync(context.UserName);
        if (user == null) 
        {
            context.Result = new GrantValidationResult(
                TokenRequestErrors.InvalidClient, "Invalid client.");
            return;
        }

        var userRoles = await _userManager.GetRolesAsync(user);
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
            authenticationMethod: "custom"
        );
    }
}