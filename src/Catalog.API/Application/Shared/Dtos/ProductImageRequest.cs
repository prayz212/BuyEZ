namespace CatalogAPI.Application.Shared.Dtos;

public record ProductImagePayload(string Filename, string URL, string AltText, int Size, bool IsPrimary = false);