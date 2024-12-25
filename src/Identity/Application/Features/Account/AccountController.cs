using Identity.Application.Features.Account;

using Shared.Common;
using Shared.Common.Constants;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Identity.Application.Account;

[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Route($"{ApiPaths.Root}/account-managements")]
public class AccountController : ApiControllerBase
{
    [HttpPost]
    [Authorize(Policy = PolicyConstants.SYSTEM_ADMIN_POLICY)]
    [ProducesResponseType(typeof(IdentityAccountDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IdentityAccountDetailResponse> AddIdentityAccount(AddIdentityAccountRequest request)
    {
        return await Mediator.Send(new AddIdentityAccountCommand(GetUserId(), request));
    }
}