using CatalogAPI.Application.Domain;
using Shared.Common.Enums;

namespace CatalogAPI.Application.Shared.Dtos;

public record ProductDetailResponse(string Id, string Name, string Description, double Price, ProductType Type, ProductStatus Status, List<ImageDetailResponse>? Images);

public static partial class ResponseExtensions
{
    public static ProductDetailResponse ToDto(this Product product)
    {
        return new(
            product.Id,
            product.Name,
            product.Description,
            product.Price,
            product.Type,
            product.Status,
            [..product.Images.Select(i => i.ToDto())]
        );
    }
}