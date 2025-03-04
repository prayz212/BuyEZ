using OrderAPI.Application.Shared.Dtos;
using OrderAPI.Application.Infrastructure.Persistence;
using OrderAPI.Application.Domain;

using Shared.Common.Exceptions;
using ValidationException = Shared.Common.Exceptions.ValidationException;

using MediatR;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
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
    ApplicationDbContext context
) : IRequestHandler<UpdateOrderCommand>
{
    private readonly ILogger<UpdateOrderCommandHandler> _logger = logger;
    private readonly ApplicationDbContext _context = context;

    public async Task Handle(UpdateOrderCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling request update order: {@Request}", request);

        if (string.IsNullOrWhiteSpace(request.CurrentUserId))
            throw new UnauthorizedAccessException("Invalid token.");

        var payload = request.Payload;
        var order = await _context.Orders.FirstOrDefaultAsync(o => 
            o.Id == payload.Id && o.CustomerId == request.CurrentUserId);
        if (order is null) throw new NotFoundException("Order not found.");

        if (!IsAllowedToUpdateCustomerInfo(order.Status))
            throw new ValidationException("Only allow to update customer info in Pending or Packaging status.");

        order.CustomerName = payload.CustomerInfo.Name;
        order.CustomerAddress = payload.CustomerInfo.Address;
        order.CustomerPhoneNumber = payload.CustomerInfo.PhoneNumber;
        order.LastModifiedBy = request.CurrentUserId;

        _logger.LogInformation("Updating order to database: {@UpdatedOrder}", order);
        _context.Orders.Update(order);
        await _context.SaveChangesAsync(cancellationToken);
    }

    private bool IsAllowedToUpdateCustomerInfo(OrderStatus status) =>
        status == OrderStatus.Pending || status == OrderStatus.Packaging;
}
