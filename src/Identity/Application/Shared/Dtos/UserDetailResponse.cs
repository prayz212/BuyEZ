using Identity.Application.Domain;

namespace Identity.Application.Shared.Dtos;

public record UserDetailResponse(string Id, string? FirstName, string? LastName, string UserName, string Email);

public static class ResponseExtensions
{
    public static UserDetailResponse ToDto(this User user) =>
        new(
            user.Id.ToString(),
            user.FirstName,
            user.LastName,
            user.UserName!,
            user.Email!
        );
}