using Identity.Application.Domain;

using MediatR;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Shared.Common.Exceptions;

using IdentityConstants = Shared.Common.Constants.IdentityConstants;

namespace Identity.Application.Features;

public record ForgotPasswordResponse(string ResetLink);
public record ForgotPasswordCommand(string PathUrl, ForgotPasswordPayload Payload) : IRequest<ForgotPasswordResponse>;

public record ForgotPasswordPayload
{
    public required string Email { get; init; }
}

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
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email address is required.")
                .EmailAddress().WithMessage("Email address is not valid.");
        }
    }
}


internal sealed class ForgotPasswordCommandHandler(
    UserManager<User> userManager
) : IRequestHandler<ForgotPasswordCommand, ForgotPasswordResponse>
{
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
        var resetLink = $"{request.PathUrl}?email={requestPayload.Email}&token={token}";

        // TODO: Send the reset link via email
        return new ForgotPasswordResponse(resetLink ?? string.Empty);
    }
}