using Shared.Common;

namespace OrderAPI.Application.Domain;

public class OrderHistory : AuditableEntity
{
    public string Id { get; init; } = string.Empty;

    public OrderStatus HistoryStatus { get; private set; }

    public string Reason { get; private set; } = string.Empty;

    public string OrderId { get; private set; } = string.Empty;

    // Constructors
    /*
        - By default, compiler will create a default parameterless constructor.
            => Compiler will NOT generate a parameterless constructor in case we already defined parameters constructor
     
        - Parameterless constructor for EF Core to instantiate the object with the data got from database
     */
    internal OrderHistory() { }

    private OrderHistory(OrderStatus status, string createdBy, string reason)
    {
        Id = Guid.NewGuid().ToString();
        HistoryStatus = status;
        Reason = reason;
        CreatedBy = createdBy;
    }

    public static OrderHistory CreateNew(OrderStatus status, string createdBy, string? reason = default)
    {
        var changedReason = reason ?? GetReasonByStatus(status);
        return new(status, createdBy, changedReason);
    }

    private static string GetReasonByStatus(OrderStatus status) => status switch
    {
        OrderStatus.Pending => "Order created",
        OrderStatus.Paid => "Payment received",
        OrderStatus.Packaging => "Order packing",
        OrderStatus.Delivering => "Order is being delivered",
        OrderStatus.Delivered => "Order delivered",
        OrderStatus.Cancelled => "Order cancelled",
        _ => throw new InvalidOperationException("Invalid OrderStatus")
    };
}
