using CatalogAPI.Application.Domain.Dtos;

using Shared.Domain;

namespace CatalogAPI.Application.Domain;

public class Image : ImageBase
{
    public bool IsPrimary { get; private set; } = false;

    public string ProductId { get; private set; } = string.Empty;

    // Constructors
    /*
        - By default, compiler will create a default parameterless constructor.
            => Compiler will NOT generate a parameterless constructor in case we already defined parameters constructor
     
        - Parameterless constructor for EF Core to instantiate the object with the data got from database
     */
    internal Image() { }

    private Image(string filename, string url, string altText, int size, bool isPrimary, string createdBy)
    {
        Id = Guid.NewGuid().ToString();
        Filename = filename;
        URL = url;
        AltText = altText;
        Size = size;
        IsPrimary = isPrimary;
        CreatedBy = createdBy;
    }

    public static Image CreateNew(ProductImagePayload productImage, string createdBy) =>
        new(
            productImage.Filename,
            productImage.URL,
            productImage.AltText,
            productImage.Size,
            productImage.IsPrimary,
            createdBy);
}