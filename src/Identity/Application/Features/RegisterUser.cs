using Identity.Application.Domain;
using Identity.Application.Infrastructure.Persistence;

using ValidationException = Shared.Common.Exceptions.ValidationException;
using IdentityConstants = Shared.Common.Constants.IdentityConstants;

using MediatR;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Identity.Application.Shared.Dtos;
using Identity.Application.Shared.Validators;

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
    ApplicationDbContext context, 
    IPasswordHasher<User> passwordHasher, 
    UserManager<User> userManager
) : IRequestHandler<RegisterUserCommand, UserDetailResponse>
{
    private readonly ApplicationDbContext _context = context;
    private readonly IPasswordHasher<User> _passwordHasher = passwordHasher;
    private readonly UserManager<User> _userManager = userManager;
    
    public async Task<UserDetailResponse> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var requestPayload = request.Payload;
        
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.UserName == requestPayload.UserName || u.Email == requestPayload.Email, cancellationToken);

        if (user?.UserName == requestPayload.UserName)
            throw new ValidationException("UserName already exists.");
        if (user?.Email == requestPayload.Email)
            throw new ValidationException("Email already exists.");
        
        var newAccount = ToEntity(requestPayload);
        newAccount.PasswordHash = _passwordHasher.HashPassword(newAccount, requestPayload.Password);
        newAccount.SecurityStamp = Guid.NewGuid().ToString();

        await _context.Users.AddAsync(newAccount, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        await _userManager.AddToRoleAsync(newAccount, IdentityConstants.Role.USER);

        // TODO: trigger an event new user created to send email notification before returning the result
        return ToDto(newAccount);
    }

    private static User ToEntity(RegisterUserPayload request) => new()
    {
        Id = Guid.NewGuid(),
        FirstName = request.FirstName,
        LastName = request.LastName,
        UserName = request.UserName,
        NormalizedUserName = request.UserName.ToUpper(),
        Email = request.Email,
        NormalizedEmail = request.Email.ToUpper(),
        PhoneNumber = request.PhoneNumber,
        Created = DateTimeOffset.UtcNow
    };

    private static UserDetailResponse ToDto(User user) => new(
        user.Id.ToString(), user.FirstName, user.LastName, user.UserName!, user.Email!
    );
}