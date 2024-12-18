using Identity.Application.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Identity.Application.Features.Identity;

[ApiController]
[Route($"{ApiPaths.Root}/identity")]
public class IdentityController : ApiControllerBase
{
    [HttpPost("token")]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<TokenResponse> GetToken(GetTokenCommand command)
    {
        return await Mediator.Send(command);
    }
}
