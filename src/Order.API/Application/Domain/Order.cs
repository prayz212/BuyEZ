using OrderAPI.Application.Domain.Dtos;
using OrderAPI.Application.Domain.Events;

using Shared.Common;
using Shared.Common.Interfaces;
using Shared.Common.Exceptions;

namespace OrderAPI.Application.Domain;

public class Order : AuditableEntity, IAggregateRoot
{
    public string Id { get; init; } = string.Empty;

    public double TotalAmount { get; private set; } = 0.0;

    public OrderStatus Status { get; private set; } = OrderStatus.Pending;

    public string CustomerId { get; private set; } = string.Empty;

    public string CustomerName { get; private set; } = string.Empty;

    public string CustomerAddress { get; private set; } = string.Empty;

    public string CustomerPhoneNumber { get; private set; } = string.Empty;

    // Navigation Properties
    private readonly List<OrderItem> _orderItems = [];
    public IReadOnlyList<OrderItem> OrderItems => _orderItems.AsReadOnly();

    private readonly List<OrderHistory> _orderHistories = [];
    public IReadOnlyList<OrderHistory> OrderHistories => _orderHistories.AsReadOnly();

    // Domain Events property
    private readonly List<DomainEvent> _domainEvents = [];
    public IReadOnlyList<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    // Constructors
    /*
        - By default, compiler will create a default parameterless constructor.
            => Compiler will NOT generate a parameterless constructor in case we already defined parameters constructor
     
        - Parameterless constructor for EF Core to instantiate the object with the data got from database
     */
    internal Order() { }

    private Order(string customerId, string customerName, string customerAddress, string customerPhoneNumber, List<OrderItem> orderItems)
    {
        Id = Guid.NewGuid().ToString();
        CustomerId = customerId;
        CustomerName = customerName;
        CustomerAddress = customerAddress;
        CustomerPhoneNumber = customerPhoneNumber;
        CreatedBy = customerId;

        _orderItems = orderItems;

        UpdateTotalAmount();
        UpdateOrderStatus(OrderStatus.Packaging); // Temporary hard coding Packaging, should be Pending

        // TODO: add more info later
        _domainEvents.Add(new OrderPlacedDomainEvent(Id));
    }

    public static Order CreateNew(
        string customerId,
        string customerName,
        string customerAddress,
        string customerPhoneNumber,
        List<OrderItemInfo> items)
    {
        if (!items.Any())
            throw new ValidationException("Order must have at least one item.");

        var orderItems = items.Select(oi => OrderItem.CreateNew(oi.Product, oi.Quantity, customerId));

        return new(
            customerId,
            customerName,
            customerAddress,
            customerPhoneNumber,
            [.. orderItems]);
    }

    public void UpdateDetails(OrderCustomerInfo customerInfo, string modifiedBy)
    {
        if (!IsAllowedToUpdateCustomerInfo(Status))
            throw new ValidationException("Only allow to update customer info in Pending or Packaging status.");

        CustomerName = customerInfo.Name;
        CustomerAddress = customerInfo.Address;
        CustomerPhoneNumber = customerInfo.PhoneNumber;
        LastModifiedBy = modifiedBy;
    }

    public void CancelOrder(string modifiedBy)
    {
        if (!IsAllowedToCancelOrder(Status))
            throw new ValidationException("Order can't be cancelled.");

        LastModifiedBy = modifiedBy;
        UpdateOrderStatus(OrderStatus.Cancelled);

        // TODO: Publish cancelled order event
    }

    public void DeliveringOrder(string modifiedBy)
    {
        if (Status != OrderStatus.Packaging)
            throw new ValidationException("Order must be in Packaging status before move to Delivering.");

        LastModifiedBy = modifiedBy;
        UpdateOrderStatus(OrderStatus.Delivering);
    }

    private void UpdateOrderStatus(OrderStatus status, string? reason = null)
    {
        Status = status;

        _orderHistories.Add(OrderHistory.CreateNew(
            status: Status,
            createdBy: LastModifiedBy ?? CreatedBy!,
            reason: reason
        ));
    }

    private bool IsAllowedToUpdateCustomerInfo(OrderStatus status) =>
        status == OrderStatus.Pending || status == OrderStatus.Packaging;

    private bool IsAllowedToCancelOrder(OrderStatus status) => 
        status == OrderStatus.Pending;

    private void UpdateTotalAmount()
    {
        TotalAmount = Math.Round(_orderItems.Sum(oi => oi.TotalPrice), 2, MidpointRounding.AwayFromZero);
    }
    
    public void AddDomainEvent(DomainEvent @event)
    {
        _domainEvents.Add(@event);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}


public enum OrderStatus
{
    Pending = 1,
    Paid,
    Packaging,
    Delivering,
    Delivered,
    Cancelled
}