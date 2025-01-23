using Identity.Application.Domain;

using MediatR;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Shared.Common.Exceptions;
using Microsoft.AspNetCore.Http;
using Identity.Application.Shared.Validators;
using ValidationException = Shared.Common.Exceptions.ValidationException;

namespace Identity.Application.Features;

public record ResetPasswordResponse(bool Successed, string Message);
public record ResetPasswordCommand(string Email, string ResetToken, ResetPasswordPayload Payload) : IRequest;

public record ResetPasswordPayload(string NewPassword);

public class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator()
    {
        RuleFor(x => x.Email).BeValidEmail();

        RuleFor(x => x.ResetToken)
            .NotNull().WithMessage("Request token is required.");

        RuleFor(x => x.Payload)
            .NotNull().WithMessage("Request payload is required.")
            .SetValidator(new ResetPasswordPayloadValidator());
    }

    class ResetPasswordPayloadValidator : AbstractValidator<ResetPasswordPayload>
    {
        public ResetPasswordPayloadValidator()
        {
            RuleFor(x => x.NewPassword).BeValidPassword();
        }
    }
}


internal sealed class ResetPasswordCommandHandler(
    UserManager<User> userManager
) : IRequestHandler<ResetPasswordCommand>
{
    private readonly UserManager<User> _userManager = userManager;
    
    public async Task Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var requestPayload = request.Payload;

        var decodedEmail = Uri.UnescapeDataString(request.Email);
        var user = await _userManager.FindByEmailAsync(decodedEmail);
        if (user == null)
        {
            throw new NotFoundException($"User with email: {decodedEmail} was not found.");
        }

        var decodedToken = Uri.UnescapeDataString(request.ResetToken);
        bool isTokenValid = await _userManager.VerifyUserTokenAsync(user, TokenOptions.DefaultProvider, "ResetPassword", decodedToken);
        if (!isTokenValid)
        {
            throw new ValidationException("The provided reset token is invalid or expired. Please request a new reset link.");
        }

        var result = await _userManager.ResetPasswordAsync(user, decodedToken, requestPayload.NewPassword);
        if (!result.Succeeded)
        {
            throw new BadHttpRequestException("Password reset failed.");
        }

        user.LastModified = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);
    }
}