using OrderAPI.Application.Domain;
using OrderAPI.Application.Domain.Dtos;
using OrderAPI.Application.Shared.Dtos;
using OrderAPI.Application.Domain.Interfaces.Repositories;

using ValidationException = Shared.Common.Exceptions.ValidationException;

using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace OrderAPI.Application.Features.Shopping;


public record AddOrderPayload(OrderCustomerInfo CustomerInfo, List<OrderProductInfo> Items);

public record AddOrderCommand(string? CurrentUserId, AddOrderPayload Payload) : IRequest<OrderDetailResponse>;


public class AddOrderCommandValidator : AbstractValidator<AddOrderCommand>
{
    public AddOrderCommandValidator()
    {
        RuleFor(x => x.Payload)
            .NotEmpty().WithMessage("Request payload is required.")
            .SetValidator(new AddOrderPayloadValidator());
    }

    class AddOrderPayloadValidator : AbstractValidator<AddOrderPayload>
    {
        public AddOrderPayloadValidator()
        {
            RuleFor(x => x.CustomerInfo.Name)
                .NotEmpty().WithMessage("Customer Name is required.");

            RuleFor(x => x.CustomerInfo.Address)
                .NotEmpty().WithMessage("Customer Address is required.");

            RuleFor(x => x.CustomerInfo.PhoneNumber)
                .NotEmpty().WithMessage("Customer Phone Number is required.")
                .Must(BeAValidPhoneNumber).WithMessage("Customer Phone Number is not valid");

            RuleForEach(x => x.Items)
                .Cascade(CascadeMode.Stop)
                .Must(BeAValidProductId).WithMessage("Product Id is not valid.")
                .Must((p) => p.Quantity >= 1).WithMessage("Product quantity must be greater than or equal 1");
        }

        // TODO: Implement validation logic
        private bool BeAValidPhoneNumber(string phoneNumber) =>
            true;

        private bool BeAValidProductId(OrderProductInfo info) => 
            Guid.TryParse(info.Id, out _);
    }
}


internal sealed class AddOrderCommandHandler : IRequestHandler<AddOrderCommand, OrderDetailResponse>
{
    private readonly ILogger<AddOrderCommandHandler> _logger;
    private readonly IProductRepository _productRepository;
    private readonly IOrderRepository _orderRepository;

    public AddOrderCommandHandler(
        ILogger<AddOrderCommandHandler> logger, 
        IProductRepository productRepository,
        IOrderRepository orderRepository)
    {
        _logger = logger;
        _productRepository = productRepository;
        _orderRepository = orderRepository;
    }

    public async Task<OrderDetailResponse> Handle(AddOrderCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling request add new order: {@Request}", request);

        // TODO: Refactor to reuse this validation
        if (string.IsNullOrWhiteSpace(request.CurrentUserId))
            throw new UnauthorizedAccessException("Invalid token.");

        var payload = request.Payload;

        // TODO: Apply Redis to cache reference products
        // TODO: Use Sagas to ensure product has enough quantity, we will skip this check at the moment            
        var productReferences = await _productRepository
            .GetByIdsAsync([..payload.Items.Select(i => i.Id)]);
        if (productReferences.Count != payload.Items.Count)
            throw new ValidationException("One or more product was not found or invalid.");

        var orderItems = from item in payload.Items
                        join productReference in productReferences
                        on item.Id equals productReference.Id
                        select new OrderItemInfo(productReference, item.Quantity);
                          
        var newOrder = Order.CreateNew(
            request.CurrentUserId,
            payload.CustomerInfo.Name,
            payload.CustomerInfo.Address,
            payload.CustomerInfo.PhoneNumber,
            [..orderItems]
        );

        _logger.LogInformation("Adding new order: {@NewOrder}", newOrder);
        await _orderRepository.AddAsync(newOrder, cancellationToken);
        await _orderRepository.SaveChangesAsync(cancellationToken);

        return newOrder.ToDto();
    }
}