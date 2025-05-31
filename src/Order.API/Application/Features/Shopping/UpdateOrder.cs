using OrderAPI.Application.Domain.Dtos;
using OrderAPI.Application.Domain.Interfaces.Repositories;

using Shared.Common.Exceptions;

using MediatR;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace OrderAPI.Application.Features.Shopping;


public record UpdateOrderPayload(string Id, OrderCustomerInfo CustomerInfo);

public record UpdateOrderCommand(string? CurrentUserId, UpdateOrderPayload Payload) : IRequest;


public class UpdateOrderCommandValidator : AbstractValidator<UpdateOrderCommand>
{
    public UpdateOrderCommandValidator()
    {
        RuleFor(x => x.Payload)
            .NotNull().WithMessage("Request payload is required.")
            .SetValidator(new UpdateOrderPayloadValidator());
    }

    class UpdateOrderPayloadValidator : AbstractValidator<UpdateOrderPayload>
    {
        public UpdateOrderPayloadValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Order Id is required.");

            RuleFor(x => x.CustomerInfo.Name)
                .NotEmpty().WithMessage("Customer Name is required.");

            RuleFor(x => x.CustomerInfo.Address)
                .NotEmpty().WithMessage("Customer Address is required.");

            RuleFor(x => x.CustomerInfo.PhoneNumber)
                .NotEmpty().WithMessage("Customer Phone Number is required.")
                .Must(BeAValidPhoneNumber).WithMessage("Customer Phone Number is not valid");
        }

        // TODO: Implement validation logic
        private bool BeAValidPhoneNumber(string phoneNumber) => 
            true;
    }
}


internal sealed class UpdateOrderCommandHandler(
    ILogger<UpdateOrderCommandHandler> logger, 
    IOrderRepository orderRepository
) : IRequestHandler<UpdateOrderCommand>
{
    private readonly ILogger<UpdateOrderCommandHandler> _logger = logger;
    private readonly IOrderRepository _orderRepository = orderRepository;

    public async Task Handle(UpdateOrderCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling request update order: {@Request}", request);

        if (string.IsNullOrWhiteSpace(request.CurrentUserId))
            throw new UnauthorizedAccessException("Invalid token.");

        var payload = request.Payload;
        var order = await _orderRepository.GetByIdAsync(payload.Id, cancellationToken);
        if (order is null) throw new NotFoundException("Order not found.");

        order.UpdateDetails(payload.CustomerInfo, request.CurrentUserId);

        _logger.LogInformation("Updating order to database: {@UpdatedOrder}", order);
        _orderRepository.Update(order);
        await _orderRepository.SaveChangesAsync(cancellationToken);
    }
}
