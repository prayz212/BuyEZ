namespace ClientManagementAPI.Application.Features.Clients.Shared.Dtos;

public record ClientImageRequest
(
    string Filename, 
    string URL, 
    string AltText, 
    int Size
);