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

public record UpdateClientCommand(string Id, string Name, string BriefDescription, SubscriptionType SubscriptionType, ProductType[] ProductTypes, DateTime ValidTo, ClientImageRequest? Logo) : IRequest;


public class UpdateClientCommandValidator : AbstractValidator<UpdateClientCommand>
{
    public UpdateClientCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100).WithMessage("Name cannot exceed 100 characters.");

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

    private bool NotContainDuplicatedTypes(ProductType[] productTypes)
    {
        return productTypes.Distinct().Count() == productTypes.Length;
    }

    private bool NotExceedAllowedProductTypes(UpdateClientCommand command)
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


internal sealed class UpdateClientCommandHandler(ApplicationDbContext context) : IRequestHandler<UpdateClientCommand>
{
    private readonly ApplicationDbContext _context = context;

    public async Task Handle(UpdateClientCommand request, CancellationToken cancellationToken)
    {
        var client = await _context.Clients
            .Include(c => c.Logo)
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);
        if (client is null)
            throw new NotFoundException($"Client with id: {request.Id} not found.");

        if (!client.IsActivated)
            throw new ValidationException($"Client with id: {request.Id} is not activated.");

        /* Perform update */
        client.Name = request.Name;
        client.BriefDescription = request.BriefDescription;
        client.SubscriptionType = request.SubscriptionType;
        client.RegisteredProductType = request.ProductTypes;
        client.ValidUntil = request.ValidTo;

        var hasChangedLogo = request.Logo?.URL != client.Logo?.URL;
        /* Remove the old logo */
        if (hasChangedLogo && client.Logo is not null)
            client.Logo = null;

        /* Add the new logo */
        if (hasChangedLogo && request.Logo is not null)
            client.Logo = ToEntity(request.Logo);

        _context.Clients.Update(client);
        await _context.SaveChangesAsync(cancellationToken);
    }

    private static Image ToEntity(ClientImageRequest request) => new()
    {
        Id = Guid.NewGuid().ToString(),
        Filename = request.Filename,
        URL = request.URL,
        AltText = request.AltText,
        Size = request.Size
    };
}