using Identity.Application.Domain;

using Shared.GrpcProto.Account;
using Shared.Common.Exceptions;
using ValidationException = Shared.Common.Exceptions.ValidationException;
using IdentityConstants = Shared.Common.Constants.IdentityConstants;

using MediatR;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

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
    ILogger<AddIdentityAccountCommandHandler> logger,
    UserManager<User> userManager
) : IRequestHandler<AddIdentityAccountCommand, IdentityAccountDetailResponse>
{
    private readonly ILogger<AddIdentityAccountCommandHandler> _logger = logger;
    private readonly UserManager<User> _userManager = userManager;
    
    public async Task<IdentityAccountDetailResponse> Handle(AddIdentityAccountCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling request add identity account: {@Request}", request);
        var requestPayload = request.Payload;

        var requestingUser = await _userManager.FindByIdAsync(requestPayload.RequestingUserId);
        if (requestingUser == null) 
            throw new NotFoundException($"Requesting user with id {requestPayload.RequestingUserId} was not found.");

        var requestingUserRole = await _userManager.GetRolesAsync(requestingUser);
        if (!requestingUserRole.Contains(IdentityConstants.Role.SYSTEM_ADMIN))
            throw new ForbiddenException();

        // TODO: call to ClientManagement to check tenant existence
        var userNameExists = await _userManager.FindByNameAsync(requestPayload.UserName) != null;
        var userEmailExists = await _userManager.FindByEmailAsync(requestPayload.Email) != null;
        if (userNameExists || userEmailExists)
            throw new ValidationException("UserName or Email already exists.");
        
        var newUser = User.CreateNew(
            requestPayload.FirstName,
            requestPayload.LastName,
            requestPayload.UserName,
            requestPayload.Email,
            tenantId: requestPayload.TenantId,
            createdBy: requestPayload.RequestingUserId
        );

        _logger.LogInformation("Adding new account to database: {@NewUser}", newUser);
        
        var createUserResult = await _userManager.CreateAsync(newUser, GenerateRandomPassword());
        if (createUserResult.Errors.Any())
            throw new Exception(createUserResult.Errors.First().Description);

        _logger.LogInformation("Adding new account's role  to database: {@AccountRole}", requestPayload.Role);

        var addUserRoleResult = await _userManager.AddToRoleAsync(newUser, requestPayload.Role);
        if (addUserRoleResult.Errors.Any())
            throw new Exception(addUserRoleResult.Errors.First().Description);
        
        return ToDto(newUser);
    }

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
    private string GenerateRandomPassword() => "passwOrd123!";
}