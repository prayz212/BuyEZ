using Identity.Application.Domain;

using MediatR;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Shared.Common.Exceptions;

using IdentityConstants = Shared.Common.Constants.IdentityConstants;
using Identity.Application.Shared.Validators;
using Identity.Application.Infrastructure.Options;
using Shared.Common;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

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
            RuleFor(x => x.Email).BeValidEmail();
        }
    }
}


internal sealed class ForgotPasswordCommandHandler(
    ILogger<ForgotPasswordCommandHandler> logger,
    UserManager<User> userManager,
    IOptions<ServiceOptions> ServiceOptions
) : IRequestHandler<ForgotPasswordCommand, ForgotPasswordResponse>
{
    private readonly ILogger<ForgotPasswordCommandHandler> _logger = logger;
    private readonly UserManager<User> _userManager = userManager;
    private readonly ServiceOptions _serviceOptions = ServiceOptions.Value;
    
    public async Task<ForgotPasswordResponse> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling request forgot password: {@Request}", request);
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

        // Generate the reset password token and the reset link
        var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
        var resetUrl = GenerateResetPasswordUrl(requestPayload.Email, resetToken);
        _logger.LogInformation("Reset password url: {ResetUrl}", resetUrl);

        // TODO: Send the reset link via email
        return new ForgotPasswordResponse(resetUrl);
    }

    private string GenerateResetPasswordUrl(string email, string token)
    {
        var baseUrl = _serviceOptions.BaseUrl;
        var apiPath = $"{ApiPaths.Root}/identity/reset-password";
        
        var encodedToken = Uri.EscapeDataString(token);
        var encodedEmail = Uri.EscapeDataString(email);

        return $"{baseUrl}/{apiPath}?email={encodedEmail}&token={encodedToken}";
    }
}