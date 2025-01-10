namespace ClientManagementAPI.Application.Shared.Dtos;

public record ClientImagePayload
(
    string Filename, 
    string URL, 
    string AltText, 
    int Size
);