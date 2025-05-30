using ClientManagementAPI.Application.Domain.Dtos;

using Shared.Domain;

namespace ClientManagementAPI.Application.Domain;

public class Image : ImageBase
{
    public string ClientId { get; set; } = string.Empty;

    // Constructors
    /*
        - By default, compiler will create a default parameterless constructor.
            => Compiler will NOT generate a parameterless constructor in case we already defined parameters constructor
     
        - Parameterless constructor for EF Core to instantiate the object with the data got from database
     */
    internal Image() { }

    private Image(string filename, string url, string altText, int size, string createdBy)
    {
        Id = Guid.NewGuid().ToString();
        Filename = filename;
        URL = url;
        AltText = altText;
        Size = size;
        CreatedBy = createdBy;
    }

    public static Image CreateNew(ClientImagePayload clientImage, string createdBy) => new
    (
        clientImage.Filename,
        clientImage.URL,
        clientImage.AltText,
        clientImage.Size,
        createdBy
    );
}