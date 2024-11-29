namespace CatalogAPI.Application.Features.Products.Shared.Dtos;

public record ProductImageRequest(string Filename, string URL, string AltText, int Size, bool IsPrimary = false);