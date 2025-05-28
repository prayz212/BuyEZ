using OrderAPI.Application.Domain;

namespace OrderAPI.Application.Shared.Dtos;

public record OrderProductInfo(string Id, int Quantity);

public record OrderItemInfo(ProductReference Product, int Quantity);