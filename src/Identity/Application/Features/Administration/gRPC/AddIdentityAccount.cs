using Identity.Application.Domain;
using Identity.Application.Infrastructure.Persistence;

using Shared.GrpcProto.Account;
using Shared.Common.Exceptions;
using ValidationException = Shared.Common.Exceptions.ValidationException;
using IdentityConstants = Shared.Common.Constants.IdentityConstants;

using MediatR;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Identity.Application.Features.Administration.gRPC;


public record AddIdentityAccountCommand(AddIdentityAccountPayload Payload) : IRequest<IdentityAccountDetailResponse>;


public class AddIdentityAccountCommandValidator : AbstractValidator<AddIdentityAccountCommand>
{
    public AddIdentityAccountCommandValidator()
    {
        RuleFor(x => x.Payload)
            .NotNull().WithMessage("Request payload is required.")
            .SetValidator(new AddIdentityAccountPayloadValidator());
    }

    class AddIdentityAccountPayloadValidator : AbstractValidator<AddIdentityAccountPayload>
    {
        public AddIdentityAccountPayloadValidator()
        {
            RuleFor(x => x.TenantId)
                .NotEmpty().WithMessage("Tenant Id is required.");

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

            RuleFor(x => x.Role)
                .NotEmpty().WithMessage("Role is required.")
                .Must(BeAValidRole).WithMessage("Role is not supported.");

            RuleFor(x => x.RequestingUserId)
                .NotEmpty().WithMessage("Requesting User Id is required.");
        }

        private bool BeAValidRole(string role) => role switch
        {
            IdentityConstants.Role.SYSTEM_ADMIN 
            or IdentityConstants.Role.SYSTEM_SUPPORT 
            or IdentityConstants.Role.TENANT_ADMIN 
            or IdentityConstants.Role.TENANT_MANAGER 
            or IdentityConstants.Role.TENANT_STAFF => true,
            _ => false
        };
    }
}


internal sealed class AddIdentityAccountCommandHandler(
    ApplicationDbContext context, 
    IPasswordHasher<User> passwordHasher, 
    UserManager<User> userManager
) : IRequestHandler<AddIdentityAccountCommand, IdentityAccountDetailResponse>
{
    private readonly ApplicationDbContext _context = context;
    private readonly IPasswordHasher<User> _passwordHasher = passwordHasher;
    private readonly UserManager<User> _userManager = userManager;
    
    public async Task<IdentityAccountDetailResponse> Handle(AddIdentityAccountCommand request, CancellationToken cancellationToken)
    {
        var requestPayload = request.Payload;

        var requestingUser = await _userManager.FindByIdAsync(requestPayload.RequestingUserId);
        if (requestingUser == null) 
            throw new NotFoundException($"Requesting user with id {requestPayload.RequestingUserId} was not found.");

        var requestingUserRole = await _userManager.GetRolesAsync(requestingUser);
        if (!requestingUserRole.Contains(IdentityConstants.Role.SYSTEM_ADMIN))
            throw new ForbiddenException();
        
        // TODO: call to ClientManagement to check tenant existence
        var isExistedUserName = await _context.Users.FirstOrDefaultAsync(u => 
            u.UserName == requestPayload.UserName && u.TenantId == requestPayload.TenantId) != null;
        if (isExistedUserName)
            throw new ValidationException("UserName already exists.");
        
        var newAccount = ToEntity(requestPayload);
        var randomPassword = GenerateRandomPassword();
        newAccount.PasswordHash = _passwordHasher.HashPassword(newAccount, randomPassword);
        newAccount.SecurityStamp = Guid.NewGuid().ToString();

        await _context.Users.AddAsync(newAccount);
        await _context.SaveChangesAsync();

        await _userManager.AddToRoleAsync(newAccount, requestPayload.Role);

        // TODO: trigger an event new user created to send email notification before returning the result
        return ToDto(newAccount);
    }

    private static User ToEntity(AddIdentityAccountPayload request) => new()
    {
        Id = Guid.NewGuid(),
        FirstName = request.FirstName,
        LastName = request.LastName,
        UserName = request.UserName,
        NormalizedUserName = request.UserName.ToUpper(),
        Email = request.Email,
        NormalizedEmail = request.Email.ToUpper(),
        TenantId = request.TenantId,
        CreatedBy = request.RequestingUserId,
        Created = DateTimeOffset.UtcNow
    };

    private static IdentityAccountDetailResponse ToDto(User user) => new()
    {
        Id = user.Id.ToString(),
        TenantId = user.TenantId!,
        FirstName = user.FirstName,
        LastName = user.LastName,
        UserName = user.UserName!,
        Email = user.Email!
    };

    // TODO: replace this with a real implementation for generating random password
    private string GenerateRandomPassword() => "password123";
}