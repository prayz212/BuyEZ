using OrderAPI.Application.Features.Shopping.Shared.Dtos;
using Shared.Common;

namespace OrderAPI.Application.Domain.Shopping;

public class OrderHistory : AuditableEntity
{
    public string Id { get; private set; } = string.Empty;

    public OrderStatus HistoryStatus { get; set; }

    public string Reason { get; set; } = string.Empty;

    public string OrderId { get; set; } = string.Empty;

    // Navigation Properties
    public Order? Order { get; set; }

    // Constructors
    /*
        - By default, compiler will create a default parameterless constructor.
            => Compiler will NOT generate a parameterless constructor in case we already defined parameters constructor
     
        - Parameterless constructor for EF Core to instantiate the object with the data got from database
     */
    internal OrderHistory() { }

    public OrderHistory(OrderStatus status, string createdBy, string? reason = null)
    {
        Id = Guid.NewGuid().ToString();
        HistoryStatus = status;
        Reason = reason ?? GetReasonByStatus(status);
        CreatedBy = createdBy;
    }

    private string GetReasonByStatus(OrderStatus status) => status switch
    {
        OrderStatus.PENDING => "Order created",
        OrderStatus.PAID => "Payment received",
        OrderStatus.PACKAGING => "Order packed ",
        OrderStatus.DELIVERING => "Order is being delivered",
        OrderStatus.DELIVERED => "Order delivered",
        OrderStatus.CANCELLED => "Order cancelled",
        _ => throw new InvalidOperationException("Invalid OrderStatus")
    };

    public static OrderHistoryResponse ToDto(OrderHistory orderHistory)
    {
        return new OrderHistoryResponse(
            orderHistory.Id,
            orderHistory.HistoryStatus,
            orderHistory.Reason,
            orderHistory.Created
        );
    }
}
