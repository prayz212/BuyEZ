namespace ClientManagementAPI.Application.Domain.Dtos;

public record ClientImagePayload(string Filename, string URL, string AltText, int Size);