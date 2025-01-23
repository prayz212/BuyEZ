namespace Identity.Application.Shared.Dtos;

public record UserDetailResponse(string Id, string? FirstName, string? LastName, string UserName, string Email);