using CatalogAPI.Application.Common;
using CatalogAPI.Application.Features.Products.Shared.Dtos;

namespace CatalogAPI.Application.Domain.Catalogs;

public class Product : AuditableEntity, IHasDomainEvent
{
    public string Id { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public decimal Price { get; set; } = 0;

    public ProductType Type { get; set; }

    public int AvailableStock { get; set; } = 0;

    public int RestockThreshold { get; set; } = 5;

    public int MaxStockThreshold { get; set; } = 10;

    public ProductStatus Status { get; set; } = ProductStatus.OutOfStock;

    public List<DomainEvent> DomainEvents { get; } = [];

    // Navigation property for the related Product
    public List<Image> Images { get; set; } = [];

    public static ProductDetailResponse ToDto(Product product) =>
        new(
            product.Id, 
            product.Name, 
            product.Description, 
            product.Price, 
            product.Type, 
            product.Status, 
            product.Images.Select(Image.ToDto).ToList()
        );
}

public enum ProductType
{
    /* CLOTHES */
    // Men
    Men_TShirt = 1,
    Men_Pants,
    Men_Jacket,

    // Women
    Women_Pants,
    Women_TShirt,
    Women_Jacket,
}

public enum ProductStatus
{
    InStock = 1,
    OutOfStock = 2,
    Restocking = 3,
}