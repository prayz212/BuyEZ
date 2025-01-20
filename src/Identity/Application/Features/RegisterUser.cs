using Identity.Application.Domain;
using Identity.Application.Infrastructure.Persistence;

using ValidationException = Shared.Common.Exceptions.ValidationException;
using IdentityConstants = Shared.Common.Constants.IdentityConstants;

using MediatR;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Identity.Application.Shared.Dtos;

namespace Identity.Application.Features;

public record RegisterUserCommand(RegisterUserPayload Payload) : IRequest<UserDetailResponse>;

public record RegisterUserPayload
{
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required string UserName { get; init; }
    public required string Email { get; init; }
    public required string Password { get; init; }
    public required string PhoneNumber { get; init; }
}

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
            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("First name is required.");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name is required.");

            RuleFor(x => x.UserName)
                .NotEmpty().WithMessage("Username is required.")
                .MinimumLength(6).WithMessage("Username must be at least 6 characters long.")
                .MaximumLength(100).WithMessage("Username must not exceed 100 characters.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email address is required.")
                .EmailAddress().WithMessage("Email address is not valid.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required.")
                .MinimumLength(8).WithMessage("Password must be at least 8 characters long.")
                .MaximumLength(16).WithMessage("Password cannot exceed 16 characters.")
                .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
                .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter.")
                .Matches(@"[0-9]").WithMessage("Password must contain at least one number.")
                .Matches(@"[\W_]").WithMessage("Password must contain at least one special character (!, @, #, $, %, &, etc.).")
                .Must(BeAValidPassword).WithMessage("Password cannot contain easily guessable words like 'password' or '123456'.")
                .NotEqual(x => x.Password.ToLower()).WithMessage("Password cannot be entirely lowercase.")
                .NotEqual(x => x.Password.ToUpper()).WithMessage("Password cannot be entirely uppercase.");

            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("Phone number is required.")
                 .Matches(@"^\d{10}$").WithMessage("Phone number must contain exactly 10 digits.");
        }

        private bool BeAValidPassword(string password)
        {
            // Add logic to reject common passwords like 'password123', '123456', etc.
            var commonPasswords = new[] { "password123", "123456", "letmein", "qwerty", "admin", "welcome" };
            return !commonPasswords.Contains(password.ToLower());
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
        
        var isExistedUserName = await _context.Users.FirstOrDefaultAsync(u => 
            u.UserName == requestPayload.UserName, cancellationToken: cancellationToken) != null;
        if (isExistedUserName)
            throw new ValidationException("UserName already exists.");

        var isExistedEmail = await _context.Users.FirstOrDefaultAsync(u => 
            u.Email == requestPayload.Email, cancellationToken: cancellationToken) != null;
        if (isExistedEmail)
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