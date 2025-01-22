using Identity.Application.Domain;

using MediatR;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Shared.Common.Exceptions;
using Microsoft.AspNetCore.Http;

namespace Identity.Application.Features;

public record ResetPasswordResponse(bool Successed, string Message);
public record ResetPasswordCommand(string Email, string ResetToken, ResetPasswordPayload Payload) : IRequest;

public record ResetPasswordPayload(string NewPassword);

public class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email address is required.")
            .EmailAddress().WithMessage("Email address is not valid.");

        RuleFor(x => x.ResetToken)
            .NotNull().WithMessage("Request reset token is required.");

        RuleFor(x => x.Payload)
            .NotNull().WithMessage("Request payload is required.")
            .SetValidator(new ResetPasswordPayloadValidator());
    }

    class ResetPasswordPayloadValidator : AbstractValidator<ResetPasswordPayload>
    {
        public ResetPasswordPayloadValidator()
        {
            RuleFor(x => x.NewPassword)
                .NotEmpty().WithMessage("Password is required.")
                .MinimumLength(8).WithMessage("Password must be at least 8 characters long.")
                .MaximumLength(16).WithMessage("Password cannot exceed 16 characters.")
                .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
                .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter.")
                .Matches(@"[0-9]").WithMessage("Password must contain at least one number.")
                .Matches(@"[\W_]").WithMessage("Password must contain at least one special character (!, @, #, $, %, &, etc.).")
                .Must(BeAValidPassword).WithMessage("Password cannot contain easily guessable words like 'password' or '123456'.")
                .NotEqual(x => x.NewPassword.ToLower()).WithMessage("Password cannot be entirely lowercase.")
                .NotEqual(x => x.NewPassword.ToUpper()).WithMessage("Password cannot be entirely uppercase.");
        }

        private bool BeAValidPassword(string password)
        {
            // Add logic to reject common passwords like 'password123', '123456', etc.
            var commonPasswords = new[] { "password123", "123456", "letmein", "qwerty", "admin", "welcome" };
            return !commonPasswords.Contains(password.ToLower());
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

        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            throw new NotFoundException($"User with email: {request.Email} was not found.");
        }

        // The ResetToken received from the request may contain spaces due to URL encoding issues.
        // Since URL encoding often replaces spaces with "%20", some tokens might also use a plus sign ('+') instead of a space.
        // To ensure the token is correctly formatted and usable, we replace any spaces (" ") in the token with "+".
        var token = request.ResetToken.Replace(" ", "+");
        var result = await _userManager.ResetPasswordAsync(user, token, requestPayload.NewPassword);
        if (!result.Succeeded && result.Errors.Any(e => e.Code == "InvalidToken"))
        {
            throw new BadHttpRequestException("The provided reset token is invalid or expired. Please request a new reset link");
        }

        user.LastModified = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);
    }
}