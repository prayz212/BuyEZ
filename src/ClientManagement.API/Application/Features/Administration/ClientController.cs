using ClientManagementAPI.Application.Shared.Dtos;

using Shared.Common;
using Shared.Common.Models;
using Shared.Common.Constants;
using Shared.Common.Exceptions;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;

namespace ClientManagementAPI.Application.Features.Administration;

[ApiController]
[ApiVersion(1)]
[Route($"{ApiPaths.Root}/client-administrations")]
public class ClientController : ApiControllerBase
{
    [HttpGet("{id}")]
    [MapToApiVersion(1)]
    [Authorize(PolicyConstants.SYSTEM_ADMIN_POLICY)]
    [ProducesResponseType(typeof(ClientDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ClientDetailResponse> Get(string id)
    {
        return await Mediator.Send(new GetClientDetailQuery(id));
    }

    [HttpPost("query")]
    [MapToApiVersion(1)]
    [Authorize(PolicyConstants.SYSTEM_ADMIN_POLICY)]
    [ProducesResponseType(typeof(PaginatedList<ClientBriefResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<PaginatedList<ClientBriefResponse>> Query(GetClientsQuery query)
    {
        return await Mediator.Send(query);
    }

    [HttpPost]
    [MapToApiVersion(1)]
    [Authorize(PolicyConstants.SYSTEM_ADMIN_POLICY)]
    [ProducesResponseType(typeof(ClientDetailResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ClientDetailResponse>> Add(AddClientPayload request)
    {
        var client = await Mediator.Send(new AddClientCommand(GetUserId(), request));
        return CreatedAtAction(nameof(Get), new { id = client.Id }, client);
    }

    [HttpPut("{id}")]
    [MapToApiVersion(1)]
    [Authorize(PolicyConstants.SYSTEM_ADMIN_POLICY)]
    [ProducesResponseType(typeof(NoContentResult), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> Update(string id, UpdateClientPayload request)
    {
        if (id != request.Id) 
            throw new ValidationException("Client Id is not correct.");

        await Mediator.Send(new UpdateClientCommand(GetUserId(), request));

        return NoContent();
    }

    /*  
        Principals  
        - There are 2 ways to activate a client:
            1. Activate through the first account creation
            2. Call this API (toggle activate in the client management page)
    */
    [HttpPut("{id}/deactivate")]
    [MapToApiVersion(1)]
    [Authorize(PolicyConstants.SYSTEM_ADMIN_POLICY)]
    [ProducesResponseType(typeof(NoContentResult), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> Deactivate(string id)
    {
        await Mediator.Send(new DeactivateClientCommand(GetUserId(), new DeactivateClientPayload(id)));
        return NoContent();
    }

    [HttpPut("{id}/activate")]
    [MapToApiVersion(1)]
    [Authorize(PolicyConstants.SYSTEM_ADMIN_POLICY)]
    [ProducesResponseType(typeof(NoContentResult), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> Activate(string id)
    {
        await Mediator.Send(new ActivateClientCommand(GetUserId(), new ActivateClientPayload(id)));
        return NoContent();
    }

    [HttpGet("tenant-info")]
    [MapToApiVersion(1)]
    [Authorize(PolicyConstants.TENANT_ADMIN_OR_MANAGER_OR_STAFF_POLICY)]
    [ProducesResponseType(typeof(ClientInfoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ClientInfoResponse> GetClientInfo()
    {
        return await Mediator.Send(new GetClientInfoQuery(GetTenantId()));
    }
}
