using CatalogAPI.Application.Domain;

namespace CatalogAPI.Application.Shared.Dtos;

public record ImageDetailResponse(string Filename, string Url, string AltText, bool IsPrimary, int Size);

public static partial class ResponseExtensions
{
    public static ImageDetailResponse ToDto(this Image image)
    {
        return new(
            image.Filename,
            image.URL,
            image.AltText,
            image.IsPrimary,
            image.Size
        );
    }
}