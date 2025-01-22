using Identity.Application.Domain;

using MediatR;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Shared.Common.Exceptions;

using IdentityConstants = Shared.Common.Constants.IdentityConstants;
using Microsoft.Extensions.Configuration;
using Identity.Application.Shared.Validators;
using Shared.Common;

namespace Identity.Application.Features;

public record ForgotPasswordResponse(string ResetUrl);
public record ForgotPasswordCommand(ForgotPasswordPayload Payload) : IRequest<ForgotPasswordResponse>;

public record ForgotPasswordPayload(string Email);

public class ForgotPasswordCommandValidator : AbstractValidator<ForgotPasswordCommand>
{
    public ForgotPasswordCommandValidator()
    {
        RuleFor(x => x.Payload)
            .NotNull().WithMessage("Request payload is required.")
            .SetValidator(new ForgotPasswordPayloadValidator());
    }

    class ForgotPasswordPayloadValidator : AbstractValidator<ForgotPasswordPayload>
    {
        public ForgotPasswordPayloadValidator()
        {
            RuleFor(x => x.Email).IsValidEmail();
        }
    }
}


internal sealed class ForgotPasswordCommandHandler(
    UserManager<User> userManager,
    IConfiguration configuration
) : IRequestHandler<ForgotPasswordCommand, ForgotPasswordResponse>
{
    private readonly IConfiguration _configuration = configuration;
    private readonly UserManager<User> _userManager = userManager;
    
    public async Task<ForgotPasswordResponse> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var requestPayload = request.Payload;

        var user = await _userManager.FindByEmailAsync(requestPayload.Email);
        if (user == null)
        {
            throw new NotFoundException($"User with email: {requestPayload.Email} was not found.");
        }

        var isCustomer = await _userManager.IsInRoleAsync(user, IdentityConstants.Role.USER);
        if (!isCustomer) 
        {
            throw new ForbiddenException("Password reset request is only permitted for customer account.");
        }   

        // Generate the password reset token and the reset link
        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var resetUrl = GenerateResetPasswordUrl(requestPayload.Email, token);

        // TODO: Send the reset link via email
        return new ForgotPasswordResponse(resetUrl);
    }

    private string GenerateResetPasswordUrl(string email, string token)
    {
        var baseUrl = _configuration["Services:BaseUrl"];
        var apiPath = $"{ApiPaths.Root}/identity/reset-password";
        
        var encodedToken = Uri.EscapeDataString(token);
        var encodedEmail = Uri.EscapeDataString(email);

        return $"{baseUrl}/{apiPath}?email={encodedEmail}&token={encodedToken}";
    }
}