using OrderAPI.Application.Domain;

namespace OrderAPI.Application.Shared.Dtos;

public record OrderItemResponse(string Id, string Name, double Price, int Quantity, double Amount);

public record OrderHistoryResponse(string Id, OrderStatus Status, string Reason, DateTimeOffset createdAt);

public record OrderDetailResponse(string Id, OrderCustomerInfo CustomerInfo, OrderStatus Status, double TotalAmount, List<OrderItemResponse> Items, List<OrderHistoryResponse> Histories, DateTimeOffset createdAt);

public static partial class ResponseExtensions
{
    public static OrderDetailResponse ToDto(this Order order)
    {
        return new(
            order.Id,
            new OrderCustomerInfo(
                order.CustomerName,
                order.CustomerAddress,
                order.CustomerPhoneNumber
            ),
            order.Status,
            order.TotalAmount,
            [..order.OrderItems.Select(oi => oi.ToDto())],
            [..order.OrderHistories.Select(oh => oh.ToDto())],
            order.Created
        );
    }

    public static OrderHistoryResponse ToDto(this OrderHistory orderHistory)
    {
        return new(
            orderHistory.Id,
            orderHistory.HistoryStatus,
            orderHistory.Reason,
            orderHistory.Created
        );
    }

    public static OrderItemResponse ToDto(this OrderItem orderItem)
    {
        return new(
            orderItem.ProductId,
            orderItem.ProductName,
            orderItem.ProductPrice,
            orderItem.Quantity,
            orderItem.TotalPrice
        );
    }
}