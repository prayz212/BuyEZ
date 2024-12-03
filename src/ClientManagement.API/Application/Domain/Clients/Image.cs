using ClientManagementAPI.Application.Features.Clients.Shared.Dtos;

namespace ClientManagementAPI.Application.Domain.Clients;

public class Image : ImageBase
{
    public string ClientId { get; set; } = string.Empty;

    public static ImageDetailResponse ToDto(Image image) => new 
    (
        image.Filename, 
        image.URL, 
        image.AltText, 
        image.Size
    );
}