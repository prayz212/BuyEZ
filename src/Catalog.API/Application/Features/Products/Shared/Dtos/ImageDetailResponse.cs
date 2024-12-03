namespace CatalogAPI.Application.Features.Products.Shared.Dtos;

public record ImageDetailResponse(string Filename, string Url, string AltText, bool IsPrimary, int Size);