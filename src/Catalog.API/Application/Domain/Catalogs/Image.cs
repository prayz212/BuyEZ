using CatalogAPI.Application.Common;
using CatalogAPI.Application.Features.Products.Shared.Dtos;

namespace CatalogAPI.Application.Domain.Catalogs;

public class Image : AuditableEntity
{
    public string Id { get; set; } = string.Empty;

    public string Filename { get; set; } = string.Empty;

    public string URL { get; set; } = string.Empty;

    public string AltText { get; set; } = string.Empty;

    public bool IsPrimary { get; set; } = false;

    public int Size { get; set; }

    public string ProductId { get; set; } = string.Empty;

    public static ImageDetailResponse ToDto(Image image) =>
        new(image.Filename, image.URL, image.AltText, image.IsPrimary, image.Size);
}