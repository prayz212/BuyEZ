using CatalogAPI.Application.Shared.Dtos;

using Shared.Domain;

namespace CatalogAPI.Application.Domain;

public class Image : ImageBase
{
    public bool IsPrimary { get; set; } = false;

    public string ProductId { get; set; } = string.Empty;

    public static ImageDetailResponse ToDto(Image image) =>
        new(image.Filename, image.URL, image.AltText, image.IsPrimary, image.Size);
}