using Identity.Application.Features.Identity.Shared.Dtos;

using Shared.Common;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Identity.Application.Features.Identity;

[ApiController]
[Route($"{ApiPaths.Root}/identity")]
public class IdentityController : ApiControllerBase
{
    [HttpPost("token")]
    [ProducesResponseType(typeof(AuthenticationTokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<AuthenticationTokenResponse> GetAuthenticationToken(GetAuthenticationTokenCommand command)
    {
        return await Mediator.Send(command);
    }

    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(AuthenticationTokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<AuthenticationTokenResponse> RefreshToken(RefreshTokenCommand command)
    {
        return await Mediator.Send(command);
    }
}
