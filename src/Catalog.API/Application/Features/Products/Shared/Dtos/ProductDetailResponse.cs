using CatalogAPI.Application.Domain.Catalogs;

namespace CatalogAPI.Application.Features.Products.Shared.Dtos;

public record ProductDetailResponse(string Id, string Name, string Description, decimal Price, ProductType Type, ProductStatus Status, List<ImageDetailResponse>? Images);