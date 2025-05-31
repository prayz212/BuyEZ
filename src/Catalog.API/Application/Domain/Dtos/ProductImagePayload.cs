namespace CatalogAPI.Application.Domain.Dtos;

public record ProductImagePayload(string Filename, string URL, string AltText, int Size, bool IsPrimary = false);