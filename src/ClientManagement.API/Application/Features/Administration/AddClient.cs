using ClientManagementAPI.Application.Options;
using ClientManagementAPI.Application.Domain;
using ClientManagementAPI.Application.Domain.Dtos;
using ClientManagementAPI.Application.Shared.Dtos;
using ClientManagementAPI.Application.Shared.Validators;
using ClientManagementAPI.Application.Domain.Interfaces.Repositories;

using Shared.Options;
using Shared.Common.Enums;
using Shared.GrpcProto.Utils;
using Shared.GrpcProto.Account;
using Shared.Common.Constants;
using ValidationException = Shared.Common.Exceptions.ValidationException;

using MediatR;
using FluentValidation;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace ClientManagementAPI.Application.Features.Administration;


public record AddClientPayload(string Name, string AliasName, string BriefDescription, SubscriptionType SubscriptionType, ProductType[] ProductTypes, ClientImagePayload? Logo);

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

            RuleForEach(x => x.ProductTypes)
                .IsInEnum().WithMessage("Invalid product types.");

            RuleFor(x => x.Logo!)
                .SetValidator(new ClientImagePayloadValidator())
                .When(x => x.Logo != null);
        }

        private bool BeAValidAlias(string aliasName)
        {
            return new Regex(@"^[0-9a-z\-]+$").IsMatch(aliasName);
        }
    }
}


internal sealed class AddClientCommandHandler : IRequestHandler<AddClientCommand, ClientDetailResponse>
{
    private readonly ILogger<AddClientCommandHandler> _logger;
    private readonly IClientRepository _clientRepository;
    private readonly IAccountService _accountService;
    private readonly GrpcBaseOptions _grpcClientOptions;

    public AddClientCommandHandler(ILogger<AddClientCommandHandler> logger, IClientRepository clientRepository, IAccountService accountService, IOptions<GrpcClientOptions> clientOptions)
    {
        _logger = logger;
        _clientRepository = clientRepository;
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
        var isExistedAliasName = await _clientRepository.CheckAliasNameExists(requestPayload.AliasName, cancellationToken);
        if (isExistedAliasName)
            throw new ValidationException("Alias Name already exists.");

        var newClient = Client.CreateNew(
            requestPayload.Name,
            requestPayload.AliasName,
            requestPayload.BriefDescription,
            requestPayload.SubscriptionType,
            requestPayload.ProductTypes,
            requestPayload.Logo,
            request.CurrentUserId);

        _logger.LogInformation("Adding new client to database: {@NewClient}", newClient);
        await _clientRepository.AddAsync(newClient, cancellationToken);
        await _clientRepository.SaveChangesAsync(cancellationToken);

        var grpcRequestPayload = GenerateGrpcRequestPayload(request.CurrentUserId, newClient);
        var callContext = GrpcUtils.GetCallOptions(_grpcClientOptions);
        
        _logger.LogInformation("Creating default tenant admin account for new client: {@NewAccount}", grpcRequestPayload);
        await _accountService.AddIdentityAccountAsync(grpcRequestPayload, callContext);

        return newClient.ToDto();
    }

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