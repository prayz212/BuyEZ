using Identity.Application.Shared.Dtos;

using Shared.Common;
using Shared.Common.Constants;

using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Identity.Application.Features;

[ApiController]
[ApiVersion(1)]
[Route($"{ApiPaths.Root}/identities")]
public class IdentityController : ApiControllerBase
{
    [HttpPost("login")]
    [MapToApiVersion(1)]
    [ProducesResponseType(typeof(AuthenticationTokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<AuthenticateUserResponse> Authenticate(AuthenticateUserQuery query)
    {
        return await Mediator.Send(query);
    }

    [HttpPost("token")]
    [MapToApiVersion(1)]
    [ProducesResponseType(typeof(AuthenticationTokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<AuthenticationTokenResponse> ExchangeCode(ExchangeAuthorizationCodeQuery query)
    {
        return await Mediator.Send(query);
    }

    [HttpPost("revoke-token")]
    [MapToApiVersion(1)]
    [ProducesResponseType(typeof(NoContentResult), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> RevokeToken(RevokeTokenQuery query)
    {
        await Mediator.Send(query);
        
        return NoContent();
    }

    [HttpPost("refresh-token")]
    [MapToApiVersion(1)]
    [ProducesResponseType(typeof(AuthenticationTokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<AuthenticationTokenResponse> RefreshToken(RefreshTokenQuery query)
    {
        return await Mediator.Send(query);
    }

    [HttpPost("register")]
    [MapToApiVersion(1)]
    [ProducesResponseType(typeof(UserDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<UserDetailResponse> Register(RegisterUserPayload payload)
    {
        return await Mediator.Send(new RegisterUserCommand(payload));
    }

    [HttpPost("forgot-password")]
    [MapToApiVersion(1)]
    [ProducesResponseType(typeof(ForgotPasswordResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ForgotPasswordResponse> ForgotPassword(ForgotPasswordPayload payload)
    {
        return await Mediator.Send(new ForgotPasswordCommand(payload));
    }

    [HttpPost("reset-password")]
    [MapToApiVersion(1)]
    [ProducesResponseType(typeof(NoContentResult), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> ResetPassword(string email, string token, ResetPasswordPayload payload)
    {
        await Mediator.Send(new ResetPasswordCommand(email, token, payload));

        return NoContent();
    }

    [HttpGet("user-info")]
    [MapToApiVersion(1)]
    [Authorize(Policy = PolicyConstants.CUSTOMER_POLICY)]
    [ProducesResponseType(typeof(GetUserInfoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<GetUserInfoResponse> GetUserInfo()
    {
        var authorization = HttpContext.Request.Headers.Authorization.First()!;
        var accessToken = authorization.Substring("Bearer ".Length).Trim();
        
        return await Mediator.Send(new GetUserInfoQuery(accessToken));
    }
}
