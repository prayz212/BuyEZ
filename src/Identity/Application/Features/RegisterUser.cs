using Identity.Application.Domain;
using Identity.Application.Shared.Dtos;
using Identity.Application.Shared.Validators;

using IdentityConstants = Shared.Common.Constants.IdentityConstants;

using MediatR;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Identity.Application.Features;


public record RegisterUserCommand(RegisterUserPayload Payload) : IRequest<UserDetailResponse>;

public record RegisterUserPayload(string FirstName, string LastName, string UserName, string Email, string Password, string PhoneNumber);


public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(x => x.Payload)
            .NotNull().WithMessage("Request payload is required.")
            .SetValidator(new RegisterUserPayloadValidator());
    }

    class RegisterUserPayloadValidator : AbstractValidator<RegisterUserPayload>
    {
        public RegisterUserPayloadValidator()
        {
            RuleFor(x => x.FirstName).BeValidFirstName();
            RuleFor(x => x.LastName).BeValidLastName();
            RuleFor(x => x.UserName).BeValidUsername();
            RuleFor(x => x.Email).BeValidEmail();
            RuleFor(x => x.Password).BeValidPassword();
            RuleFor(x => x.PhoneNumber).BeValidPhoneNumber();
        }
    }
}


internal sealed class RegisterUserCommandHandler(
    ILogger<RegisterUserCommandHandler> logger, 
    UserManager<User> userManager
) : IRequestHandler<RegisterUserCommand, UserDetailResponse>
{
    private readonly ILogger<RegisterUserCommandHandler> _logger = logger;
    private readonly UserManager<User> _userManager = userManager;
    
    public async Task<UserDetailResponse> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling request register user: {@Request}", request with { Payload = request.Payload with { Password = "###" }});
        var requestPayload = request.Payload;

        var userNameExists = await _userManager.FindByNameAsync(requestPayload.UserName) != null;
        var userEmailExists = await _userManager.FindByEmailAsync(requestPayload.Email) != null;
        if (userNameExists || userEmailExists)
            throw new ValidationException("UserName or Email already exists.");

        var newUser = User.CreateNew(
            requestPayload.FirstName,
            requestPayload.LastName,
            requestPayload.UserName,
            requestPayload.Email,
            requestPayload.PhoneNumber
        );

        _logger.LogInformation("Adding new account to database: {@NewUser}", newUser);

        var createUserResult = await _userManager.CreateAsync(newUser, requestPayload.Password);
        if (createUserResult.Errors.Any())
            throw new Exception(createUserResult.Errors.First().Description);

        var addUserRoleResult = await _userManager.AddToRoleAsync(newUser, IdentityConstants.Role.USER);
        if (addUserRoleResult.Errors.Any())
            throw new Exception(addUserRoleResult.Errors.First().Description);

        return newUser.ToDto();
    }
}