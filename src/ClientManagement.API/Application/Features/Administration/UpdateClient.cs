using ClientManagementAPI.Application.Domain;
using ClientManagementAPI.Application.Domain.Dtos;
using ClientManagementAPI.Application.Shared.Validators;
using ClientManagementAPI.Application.Domain.Interfaces.Repositories;

using Shared.Common.Enums;
using Shared.Common.Exceptions;
using ValidationException = Shared.Common.Exceptions.ValidationException;

using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClientManagementAPI.Application.Features.Administration;


public record UpdateClientPayload(string Id, string Name, string BriefDescription, SubscriptionType SubscriptionType, ProductType[] ProductTypes, ClientImagePayload? Logo);

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

            RuleForEach(x => x.ProductTypes)
                .IsInEnum().WithMessage("Invalid product types.");

            RuleFor(x => x.Logo!)
                .SetValidator(new ClientImagePayloadValidator())
                .When(x => x.Logo != null);
        }
    }
}


internal sealed class UpdateClientCommandHandler(ILogger<UpdateClientCommandHandler> logger, IClientRepository clientRepository) 
    : IRequestHandler<UpdateClientCommand>
{
    private readonly ILogger<UpdateClientCommandHandler> _logger = logger;
    private readonly IClientRepository _clientRepository = clientRepository;

    public async Task Handle(UpdateClientCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling request update client: {@Request}", request);

        // TODO: refactor to reuse this validation
        if (string.IsNullOrWhiteSpace(request.CurrentUserId))
            throw new UnauthorizedAccessException("Invalid token.");

        var requestPayload = request.Payload;
        var client = await _clientRepository.GetByIdAsync(requestPayload.Id, cancellationToken);
        if (client is null)
            throw new NotFoundException($"Client with id: {requestPayload.Id} not found.");

        /* Perform update */
        client.UpdateDetails(
            requestPayload.Name,
            requestPayload.BriefDescription,
            requestPayload.SubscriptionType,
            requestPayload.ProductTypes,
            requestPayload.Logo,
            request.CurrentUserId);

        _logger.LogInformation("Updating client to database: {@UpdatedClient}", client);
        _clientRepository.Update(client);
        await _clientRepository.SaveChangesAsync(cancellationToken);
    }
}