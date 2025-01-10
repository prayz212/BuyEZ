using OrderAPI.Application.Shared.Dtos;
using Shared.Common;

namespace OrderAPI.Application.Domain;

public class Order : AuditableEntity, IHasDomainEvent
{
    public string Id { get; private set; } = string.Empty;

    public double TotalAmount { get; private set; } = 0.0;

    public OrderStatus Status { get; private set; } = OrderStatus.Pending;

    public string CustomerId { get; private set; } = string.Empty;

    public string CustomerName { get; set; } = string.Empty;

    public string CustomerAddress { get; set; } = string.Empty;

    public string CustomerPhoneNumber { get; set; } = string.Empty;

    public List<DomainEvent> DomainEvents { get; } = [];

    // Navigation Properties
    private readonly List<OrderItem> _orderItems = [];
    public IReadOnlyList<OrderItem> OrderItems => _orderItems.AsReadOnly();

    private readonly List<OrderHistory> _orderHistories = [];
    public IReadOnlyList<OrderHistory> OrderHistories => _orderHistories.AsReadOnly();

    // Constructors
    /*
        - By default, compiler will create a default parameterless constructor.
            => Compiler will NOT generate a parameterless constructor in case we already defined parameters constructor
     
        - Parameterless constructor for EF Core to instantiate the object with the data got from database
     */
    internal Order() { }

    public Order(string customerId, string customerName, string customerAddress, string customerPhoneNumber, string createdBy)
    {
        Id = Guid.NewGuid().ToString();
        CustomerId = customerId;
        CustomerName = customerName;
        CustomerAddress = customerAddress;
        CustomerPhoneNumber = customerPhoneNumber;
        CreatedBy = createdBy;

        UpdateOrderStatus(OrderStatus.Pending);
    }

    public void AddOrderItems(List<OrderItem> orderItems)
    {
        _orderItems.AddRange(orderItems);
        UpdateTotalAmount();
    }

    public void UpdateOrderStatus(OrderStatus status, string? reason = null)
    {
        Status = status;

        _orderHistories.Add(new OrderHistory(
            status: Status, 
            createdBy: LastModifiedBy ?? CreatedBy!, 
            reason: reason
        ));
    }

    private void UpdateTotalAmount()
    {
        TotalAmount = Math.Round(_orderItems.Sum(oi => oi.TotalPrice), 2, MidpointRounding.AwayFromZero);
    }

    public static OrderDetailResponse ToDto(Order order)
    {
        return new OrderDetailResponse(
            order.Id,
            new OrderCustomerInfo(
                order.CustomerName,
                order.CustomerAddress,
                order.CustomerPhoneNumber
            ),
            order.Status,
            order.TotalAmount,
            order.OrderItems.Select(OrderItem.ToDto).ToList(),
            order.OrderHistories.Select(OrderHistory.ToDto).ToList(),
            order.Created
        );
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