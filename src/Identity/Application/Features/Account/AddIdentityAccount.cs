using Identity.Application.Domain.Identity;
using Identity.Application.Infrastructure.Persistence;

using ValidationException = Shared.Common.Exceptions.ValidationException;

using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Identity.Application.Features.Account;

public record AddIdentityAccountRequest(string? TenantId, string FirstName, string LastName, string UserName, string Email, string? Password);

public record AddIdentityAccountCommand(string? CurrentUserId, AddIdentityAccountRequest Payload) : IRequest<IdentityAccountDetailResponse>;

public record IdentityAccountDetailResponse(string? TenantId, string Id, string FirstName, string LastName, string UserName, string Email);


public class AddIdentityAccountCommandValidator : AbstractValidator<AddIdentityAccountCommand>
{
    public AddIdentityAccountCommandValidator()
    {
        RuleFor(x => x.Payload)
            .NotNull().WithMessage("Request payload is required.")
            .SetValidator(new AddIdentityAccountRequestValidator());
    }

    class AddIdentityAccountRequestValidator : AbstractValidator<AddIdentityAccountRequest>
    {
        public AddIdentityAccountRequestValidator()
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

            // TODO: validate password
        }
    }
}


internal sealed class AddIdentityAccountCommandHandler(ApplicationDbContext context, IPasswordHasher<User> passwordHasher) : IRequestHandler<AddIdentityAccountCommand, IdentityAccountDetailResponse>
{
    private readonly ApplicationDbContext _context = context;
    private readonly IPasswordHasher<User> _passwordHasher = passwordHasher;
    
    public async Task<IdentityAccountDetailResponse> Handle(AddIdentityAccountCommand request, CancellationToken cancellationToken)
    {
        // TODO: refactor to reuse this validation
        if (string.IsNullOrWhiteSpace(request.CurrentUserId))
            throw new UnauthorizedAccessException("Invalid token.");

        var requestPayload = request.Payload;

        var isExistedUserName = await _context.Users.FirstOrDefaultAsync(u => u.UserName == requestPayload.UserName && u.TenantId == requestPayload.TenantId) != null;
        if (isExistedUserName)
            throw new ValidationException("UserName already exists.");
        
        var newAccount = ToEntity(request.CurrentUserId, requestPayload);
        newAccount.PasswordHash = _passwordHasher.HashPassword(newAccount, requestPayload.Password ?? GenerateRandomPassword());
        newAccount.SecurityStamp = Guid.NewGuid().ToString();

        await _context.Users.AddAsync(newAccount);
        await _context.SaveChangesAsync();

        return User.ToDto(newAccount);
    }

    private static User ToEntity(string createdBy,AddIdentityAccountRequest request) => new()
    {
        Id = Guid.NewGuid(),
        FirstName = request.FirstName,
        LastName = request.LastName,
        UserName = request.UserName,
        NormalizedUserName = request.UserName.ToUpper(),
        Email = request.Email,
        NormalizedEmail = request.Email.ToUpper(),
        CreatedBy = createdBy,
        
        Created = DateTimeOffset.UtcNow
    };

    // TODO: Implement random password generation
    private static string GenerateRandomPassword() => "password123";
}