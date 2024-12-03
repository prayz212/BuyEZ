namespace ClientManagementAPI.Application.Features.Clients.Shared.Dtos;

public record ImageDetailResponse(string Filename, string Url, string AltText, int Size);