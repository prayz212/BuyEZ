namespace OrderAPI.Application.Domain.Dtos;

public record OrderItemInfo(ProductReference Product, int Quantity);