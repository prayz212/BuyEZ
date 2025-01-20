using Identity.Application.Shared.Dtos;

using Shared.Common;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Identity.Application.Features;

[ApiController]
[Route($"{ApiPaths.Root}/identity")]
public class IdentityController : ApiControllerBase
{
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthenticationTokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<AuthenticateUserResponse> Authenticate(AuthenticateUserQuery query)
    {
        return await Mediator.Send(query);
    }

    [HttpPost("token")]
    [ProducesResponseType(typeof(AuthenticationTokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<AuthenticationTokenResponse> ExchangeCode(ExchangeAuthorizationCodeQuery query)
    {
        return await Mediator.Send(query);
    }

    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(AuthenticationTokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<AuthenticationTokenResponse> RefreshToken(RefreshTokenQuery query)
    {
        return await Mediator.Send(query);
    }

    [HttpPost("register")]
    [ProducesResponseType(typeof(UserDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<UserDetailResponse> Register(RegisterUserCommand command)
    {
        return await Mediator.Send(command);
    }

    [HttpPost("forgot-password")]
    [ProducesResponseType(typeof(ForgotPasswordResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ForgotPasswordResponse> RequestPasswordReset(ForgotPasswordPayload payload)
    {
        var pathUrl = $"{GetBaseUrl()}/{ApiPaths.Root}/identity/reset-password";
        return await Mediator.Send(new ForgotPasswordCommand(pathUrl, payload));
    }

    [HttpPost("reset-password")]
    [ProducesResponseType(typeof(ResetPasswordResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ResetPasswordResponse> ResetPasswordReset(string email, string token, ResetPasswordPayload payload)
    {
        return await Mediator.Send(new ResetPasswordCommand(email, token, payload));
    }
}
