using ClientManagementAPI.Application.Domain;
using ClientManagementAPI.Application.Shared.Common;
using ClientManagementAPI.Application.Shared.Dtos;
using ClientManagementAPI.Application.Shared.Validators;
using ClientManagementAPI.Application.Infrastructure.Persistence;

using Shared.Common.Enums;
using Shared.Common.Exceptions;
using ValidationException = Shared.Common.Exceptions.ValidationException;

using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClientManagementAPI.Application.Features.Administration;


public record UpdateClientPayload(string Id, string Name, string BriefDescription, SubscriptionType SubscriptionType, ProductType[] ProductTypes, DateTime ValidTo, ClientImagePayload? Logo);

public record UpdateClientCommand(string? CurrentUserId, UpdateClientPayload Payload) : IRequest;


public class UpdateClientCommandValidator : AbstractValidator<UpdateClientCommand>
{
    public UpdateClientCommandValidator()
    {
        RuleFor(x => x.Payload)
            .NotNull().WithMessage("Request payload is required.")
            .SetValidator(new UpdateClientPayloadValidator());
    }

    class UpdateClientPayloadValidator : AbstractValidator<UpdateClientPayload>
    {
        public UpdateClientPayloadValidator()
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
                .SetValidator(new ClientImagePayloadValidator())
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

        private bool NotExceedAllowedProductTypes(UpdateClientPayload request)
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


internal sealed class UpdateClientCommandHandler(ApplicationDbContext context) : IRequestHandler<UpdateClientCommand>
{
    private readonly ApplicationDbContext _context = context;

    public async Task Handle(UpdateClientCommand request, CancellationToken cancellationToken)
    {
        // TODO: refactor to reuse this validation
        if (string.IsNullOrWhiteSpace(request.CurrentUserId))
            throw new UnauthorizedAccessException("Invalid token.");

        var requestPayload = request.Payload;
        var client = await _context.Clients
            .Include(c => c.Logo)
            .FirstOrDefaultAsync(c => c.Id == requestPayload.Id, cancellationToken);
        if (client is null)
            throw new NotFoundException($"Client with id: {requestPayload.Id} not found.");

        if (!client.IsActivated)
            throw new ValidationException($"Client with id: {requestPayload.Id} is not activated.");

        /* Perform update */
        client.Name = requestPayload.Name;
        client.BriefDescription = requestPayload.BriefDescription;
        client.SubscriptionType = requestPayload.SubscriptionType;
        client.RegisteredProductType = requestPayload.ProductTypes;
        client.ValidUntil = requestPayload.ValidTo;
        client.LastModifiedBy = request.CurrentUserId;

        var hasChangedLogo = requestPayload.Logo?.URL != client.Logo?.URL;
        /* Remove the old logo */
        if (hasChangedLogo && client.Logo is not null)
            client.Logo = null;

        /* Add the new logo */
        if (hasChangedLogo && requestPayload.Logo is not null)
            client.Logo = ToEntity(request.CurrentUserId, requestPayload.Logo);

        _context.Clients.Update(client);
        await _context.SaveChangesAsync(cancellationToken);
    }

    private static Image ToEntity(string modifiedBy, ClientImagePayload request) => new()
    {
        Id = Guid.NewGuid().ToString(),
        Filename = request.Filename,
        URL = request.URL,
        AltText = request.AltText,
        Size = request.Size,
        LastModifiedBy = modifiedBy
    };
}