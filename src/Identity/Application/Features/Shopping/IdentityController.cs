using Identity.Application.Shared.Dtos;

using Shared.Common;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Identity.Application.Features.Shopping;

[ApiController]
[Route($"{ApiPaths.Root}/identity")]
public class IdentityController : ApiControllerBase
{
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthenticationTokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<AuthenticateUserResponse> Authenticate(AuthenticateUserQuery command)
    {
        return await Mediator.Send(command);
    }

    [HttpPost("token")]
    [ProducesResponseType(typeof(AuthenticationTokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<AuthenticationTokenResponse> GetAuthenticationToken(GetAuthenticationTokenQuery command)
    {
        return await Mediator.Send(command);
    }

    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(AuthenticationTokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<AuthenticationTokenResponse> RefreshToken(RefreshTokenQuery command)
    {
        return await Mediator.Send(command);
    }
}
