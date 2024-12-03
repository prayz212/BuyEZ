using System.Text.RegularExpressions;
using ClientManagementAPI.Application.Common.Exceptions;
using ClientManagementAPI.Application.Domain.Clients;
using ClientManagementAPI.Application.Features.Clients.Shared.Common;
using ClientManagementAPI.Application.Features.Clients.Shared.Dtos;
using ClientManagementAPI.Application.Features.Clients.Shared.Validators;
using ClientManagementAPI.Application.Infrastructure.Persistence;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ValidationException = ClientManagementAPI.Application.Common.Exceptions.ValidationException;

namespace ClientManagementAPI.Application.Features.Clients;

public record AddClientCommand(string Name, string AliasName, string BriefDescription, SubscriptionType SubscriptionType, ProductType[] ProductTypes, DateTime ValidTo, ClientImageRequest? Logo)
    : IRequest<ClientDetailResponse>;


public class AddClientCommandValidator : AbstractValidator<AddClientCommand>
{
    public AddClientCommandValidator()
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
            .SetValidator(new ClientImageRequestValidator())
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

    private bool NotExceedAllowedProductTypes(AddClientCommand command)
    {
        switch (command.SubscriptionType)
        {
            case SubscriptionType.Basic:
                return command.ProductTypes.Length <= ClientConstants.MAXIMUM_PRODUCT_TYPES_BASIC_SUB;
            case SubscriptionType.Standard:
                return command.ProductTypes.Length <= ClientConstants.MAXIMUM_PRODUCT_TYPES_STANDARD_SUB;
            case SubscriptionType.Premium:
                return true;
            default:
                return false;
        }
    }
}


internal sealed class AddClientCommandHandler(ApplicationDbContext context) : IRequestHandler<AddClientCommand, ClientDetailResponse>
{
    private readonly ApplicationDbContext _context = context;

    public async Task<ClientDetailResponse> Handle(AddClientCommand request, CancellationToken cancellationToken)
    {
        var isExistedAliasName = await _context.Clients.AnyAsync(c => c.AliasName == request.AliasName);
        if (isExistedAliasName)
            throw new ValidationException("Alias Name already exists.");

        var newClient = ToEntity(request);
        if (request.Logo != null)
        {
            var clientLogo = ToEntity(request.Logo);
            newClient.Logo = clientLogo;
        }

        await _context.Clients.AddAsync(newClient, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return Client.ToDto(newClient);
    }

    private static Client ToEntity(AddClientCommand client) => new()
    {
        Id = Guid.NewGuid().ToString(),
        Name = client.Name,
        AliasName = client.AliasName,
        BriefDescription = client.BriefDescription,
        SubscriptionType = client.SubscriptionType,
        RegisteredProductType = client.ProductTypes,
        ValidUntil = client.ValidTo,
        IsActivated = false
    };

    private static Image ToEntity(ClientImageRequest clientImage) => new()
    {
        Id = Guid.NewGuid().ToString(),
        Filename = clientImage.Filename,
        URL = clientImage.URL,
        AltText = clientImage.AltText,
        Size = clientImage.Size,
    };
}