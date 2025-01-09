using CatalogAPI.Application.Domain;

namespace CatalogAPI.Application.Shared.Dtos;

public record ProductDetailResponse(string Id, string Name, string Description, double Price, ProductType Type, ProductStatus Status, List<ImageDetailResponse>? Images);