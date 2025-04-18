using OrderAPI.Application.Domain;

namespace OrderAPI.Application.Shared.Dtos;

public record OrderItemResponse(string Id, string Name, double Price, int Quantity, double Amount);

public record OrderHistoryResponse(string Id, OrderStatus Status, string Reason, DateTimeOffset createdAt);

public record OrderDetailResponse(string Id, OrderCustomerInfo CustomerInfo, OrderStatus Status, double TotalAmount, List<OrderItemResponse> Items, List<OrderHistoryResponse> Histories, DateTimeOffset createdAt);