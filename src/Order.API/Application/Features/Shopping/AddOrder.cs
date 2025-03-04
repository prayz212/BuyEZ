using OrderAPI.Application.Options;
using OrderAPI.Application.Domain;
using OrderAPI.Application.Shared.Dtos;
using OrderAPI.Application.Infrastructure.Persistence;

using Shared.Options;
using Shared.GrpcProto.Utils;
using Shared.GrpcProto.Catalog;
using ValidationException = Shared.Common.Exceptions.ValidationException;

using FluentValidation;
using MediatR;
using Microsoft.Extensions.Options;
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
    private readonly ApplicationDbContext _context;
    private readonly ICatalogService _catalogService;
    private readonly GrpcBaseOptions _grpcClientOptions;

    public AddOrderCommandHandler(
        ILogger<AddOrderCommandHandler> logger, 
        ApplicationDbContext context, 
        ICatalogService catalogService, 
        IOptions<GrpcClientOptions> clientOptions)
    {
        _logger = logger;
        _context = context;
        _catalogService = catalogService;
        _grpcClientOptions = clientOptions.Value.Catalog;
    }

    public async Task<OrderDetailResponse> Handle(AddOrderCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling request add new order: {@Request}", request);

        // TODO: Refactor to reuse this validation
        if (string.IsNullOrWhiteSpace(request.CurrentUserId))
            throw new UnauthorizedAccessException("Invalid token.");

        var payload = request.Payload;

        var itemsDictionary = payload.Items.ToDictionary(
            p => p.Id, 
            p => p.Quantity
        );

        // TODO: Apply Redis to cache reference products
        // TODO: Apply gRPC retry logic
        var grpcRequestPayload = GenerateGrpcRequestPayload(payload.Items); 
        var callContext = GrpcUtils.GetCallOptions(_grpcClientOptions);

        _logger.LogInformation("Calling to Catalog to get order items info: {@Payload}", grpcRequestPayload);
        var orderItems = await _catalogService.GetOrderItemsAsync(grpcRequestPayload, callContext);

        EnsureHasEnoughProductQuantity(orderItems, itemsDictionary);

        var newOrder = ToEntity(payload, request.CurrentUserId);
        var newOrderItems = orderItems.Products
            .Select(p => ToEntity(p, itemsDictionary[p.Id], request.CurrentUserId))
            .ToList();

        newOrder.AddOrderItems(newOrderItems);

        _logger.LogInformation("Adding new order: {@NewOrder}", newOrder);
        await _context.Orders.AddAsync(newOrder, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        // TODO: Publish new order event
        return Order.ToDto(newOrder);
    }

    public static void EnsureHasEnoughProductQuantity(GetOrderItemsResponse orderItems, Dictionary<string, int> itemsDictionary)
    {
        if (!orderItems.IsEnough)
            throw new ValidationException("Some of ordered product is not available.");

        var insufficientProducts = orderItems.Products
            .Where(p => p.AvailableStock < itemsDictionary[p.Id])
            .ToList();

        if (insufficientProducts.Any())
        {
            var insufficientProductIds = string.Join(", ", insufficientProducts.Select(p => p.Id));
            throw new ValidationException($"The following product id(s) is not available or out of stock: {insufficientProductIds}.");
        }
    }

    private GetOrderItemsPayload GenerateGrpcRequestPayload(List<OrderProductInfo> productInfos) => 
        new() { Ids = productInfos.Select(p => p.Id).ToList() };

    private Order ToEntity(AddOrderPayload request, string currentUserId)
    {
        return new Order(
            customerId: currentUserId,
            customerName: request.CustomerInfo.Name,
            customerAddress: request.CustomerInfo.Address,
            customerPhoneNumber: request.CustomerInfo.PhoneNumber,
            createdBy: currentUserId
        );
    }

    private OrderItem ToEntity(ProductReference productReference, int quantity, string currentUserId)
    {
        return new OrderItem(
            productId: productReference.Id,
            productName: productReference.Name,
            productPrice: productReference.Price,
            quantity: quantity,
            createdBy: currentUserId
        );
    }
}