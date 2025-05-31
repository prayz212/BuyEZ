using ClientManagementAPI.Application.Domain;

namespace ClientManagementAPI.Application.Shared.Dtos;

public record ImageDetailResponse(string Filename, string Url, string AltText, int Size);

public static partial class ResponseExtensions
{
    public static ImageDetailResponse ToDto(this Image image) =>
    new
    (
        image.Filename,
        image.URL,
        image.AltText,
        image.Size
    );
}