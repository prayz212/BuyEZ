using ClientManagementAPI.Application.Options;
using ClientManagementAPI.Application.Domain;
using ClientManagementAPI.Application.Shared.Common;
using ClientManagementAPI.Application.Shared.Dtos;
using ClientManagementAPI.Application.Shared.Validators;
using ClientManagementAPI.Application.Infrastructure.Persistence;

using Shared.Options;
using Shared.Common.Enums;
using Shared.GrpcProto.Utils;
using Shared.GrpcProto.Account;
using Shared.Common.Constants;
using ValidationException = Shared.Common.Exceptions.ValidationException;

using MediatR;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace ClientManagementAPI.Application.Features.Administration;


public record AddClientPayload(string Name, string AliasName, string BriefDescription, SubscriptionType SubscriptionType, ProductType[] ProductTypes, DateTime ValidTo, ClientImagePayload? Logo);

public record AddClientCommand(string? CurrentUserId, AddClientPayload Payload) : IRequest<ClientDetailResponse>;


public class AddClientCommandValidator : AbstractValidator<AddClientCommand>
{
    public AddClientCommandValidator()
    {
        RuleFor(x => x.Payload)
            .NotNull().WithMessage("Request payload is required.")
            .SetValidator(new AddClientPayloadValidator());
    }

    class AddClientPayloadValidator : AbstractValidator<AddClientPayload>
    {
        public AddClientPayloadValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required.")
                .MaximumLength(100).WithMessage("Name cannot exceed 100 characters.");

            RuleFor(x => x.AliasName)
                .NotEmpty().WithMessage("Alias Name is required.")
                .MaximumLength(100).WithMessage("Alias Name cannot exceed 100 characters.")
                .Must(BeAValidAlias).WithMessage("Alias Name just allowed to contains lower characters and a hyphen between them.");

            RuleFor(x => x.BriefDescription)
                .NotEmpty().WithMessage("Brief Description is required.")
                .MaximumLength(512).WithMessage("Brief Description cannot exceed 512 characters.");

            RuleFor(x => x.SubscriptionType)
                .IsInEnum().WithMessage("Invalid subscription type.");

            RuleFor(x => x.ProductTypes)
                .NotEmpty().WithMessage("At least one product type must be selected.")
                .Must(NotContainDuplicatedTypes).WithMessage("Contains duplicate product types.");

            RuleForEach(x => x.ProductTypes)
                .IsInEnum().WithMessage("Invalid product types.");

            RuleFor(x => x.ValidTo)
                .NotEmpty().WithMessage("Valid To is required.")
                .GreaterThan(DateTime.Now).WithMessage("Valid date must be greater than current datetime.");

            RuleFor(x => x.Logo!)
                .SetValidator(new ClientImagePayloadValidator())
                .When(x => x.Logo != null);

            RuleFor(x => x)
                .Must(NotExceedAllowedProductTypes)
                .WithMessage("Exceeded the maximum allowed product types for current subscription.")
                .OverridePropertyName("ProductTypes");
        }

        private bool BeAValidAlias(string aliasName)
        {
            return new Regex(@"^[0-9a-z\-]+$").IsMatch(aliasName);
        }

        private bool NotContainDuplicatedTypes(ProductType[] productTypes)
        {
            return productTypes.Distinct().Count() == productTypes.Length;
        }

        private bool NotExceedAllowedProductTypes(AddClientPayload request)
        {
            switch (request.SubscriptionType)
            {
                case SubscriptionType.Basic:
                    return request.ProductTypes.Length <= ClientConstants.MAXIMUM_PRODUCT_TYPES_BASIC_SUB;
                case SubscriptionType.Standard:
                    return request.ProductTypes.Length <= ClientConstants.MAXIMUM_PRODUCT_TYPES_STANDARD_SUB;
                case SubscriptionType.Premium:
                    return true;
                default:
                    return false;
            }
        }
    }
}


internal sealed class AddClientCommandHandler : IRequestHandler<AddClientCommand, ClientDetailResponse>
{
    private readonly ILogger<AddClientCommandHandler> _logger;
    private readonly ApplicationDbContext _context;
    private readonly IAccountService _accountService;
    private readonly GrpcBaseOptions _grpcClientOptions;

    public AddClientCommandHandler(ILogger<AddClientCommandHandler> logger, ApplicationDbContext context, IAccountService accountService, IOptions<GrpcClientOptions> clientOptions)
    {
        _logger = logger;
        _context = context;
        _accountService = accountService;
        _grpcClientOptions = clientOptions.Value.Identity;
    }

    public async Task<ClientDetailResponse> Handle(AddClientCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling request add new client: {@Request}", request);

        // TODO: refactor to reuse this validation
        if (string.IsNullOrWhiteSpace(request.CurrentUserId))
            throw new UnauthorizedAccessException("Invalid token.");

        var requestPayload = request.Payload;
        var isExistedAliasName = await _context.Clients.AnyAsync(c => c.AliasName == requestPayload.AliasName);
        if (isExistedAliasName)
            throw new ValidationException("Alias Name already exists.");

        var newClient = ToEntity(request.CurrentUserId, request.Payload);
        if (requestPayload.Logo != null)
        {
            var clientLogo = ToEntity(request.CurrentUserId, requestPayload.Logo);
            newClient.Logo = clientLogo;
        }

        _logger.LogInformation("Adding new client to database: {@NewClient}", newClient);
        await _context.Clients.AddAsync(newClient, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        var grpcRequestPayload = GenerateGrpcRequestPayload(request.CurrentUserId, newClient);
        var callContext = GrpcUtils.GetCallOptions(_grpcClientOptions);
        
        _logger.LogInformation("Creating default tenant admin account for new client: {@NewAccount}", grpcRequestPayload);
        await _accountService.AddIdentityAccountAsync(grpcRequestPayload, callContext);

        return Client.ToDto(newClient);
    }

    private static Client ToEntity(string createdBy, AddClientPayload client) => new()
    {
        Id = Guid.NewGuid().ToString(),
        Name = client.Name,
        AliasName = client.AliasName,
        BriefDescription = client.BriefDescription,
        SubscriptionType = client.SubscriptionType,
        RegisteredProductType = client.ProductTypes,
        ValidUntil = client.ValidTo,
        IsActivated = false,
        CreatedBy = createdBy
    };

    private static Image ToEntity(string createdBy, ClientImagePayload clientImage) => new()
    {
        Id = Guid.NewGuid().ToString(),
        Filename = clientImage.Filename,
        URL = clientImage.URL,
        AltText = clientImage.AltText,
        Size = clientImage.Size,
        CreatedBy = createdBy
    };

    private static AddIdentityAccountPayload GenerateGrpcRequestPayload(string currentUserId, Client newClient)
    {
        var clientNameUnderscore = newClient.Name.Trim().ToLower().Replace(" ", "_");
        return new()
        {
            TenantId = newClient.Id,
            RequestingUserId = currentUserId,
            FirstName = newClient.Name,
            LastName = "Administrator",
            UserName = $"{clientNameUnderscore}_administrator",
            Email = $"{clientNameUnderscore}_administrator@gmail.com",
            Role = IdentityConstants.Role.TENANT_ADMIN
        };
    }
}