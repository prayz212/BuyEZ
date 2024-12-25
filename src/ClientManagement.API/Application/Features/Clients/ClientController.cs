using ClientManagementAPI.Application.Features.Clients.Shared.Dtos;

using Shared.Common;
using Shared.Common.Models;
using Shared.Common.Constants;
using Shared.Common.Exceptions;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ClientManagementAPI.Application.Features.Clients;

[ApiController]
[Authorize(PolicyConstants.SYSTEM_ADMIN_POLICY)]
[Route($"{ApiPaths.Root}/client-managements")]
public class ClientController : ApiControllerBase
{
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ClientDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ClientDetailResponse> Get(string id)
    {
        return await Mediator.Send(new GetClientRequest(id));
    }

    [HttpPost("query")]
    [ProducesResponseType(typeof(PaginatedList<ClientBriefResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<PaginatedList<ClientBriefResponse>> Query(QueryClientRequest query)
    {
        return await Mediator.Send(query);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ClientDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ClientDetailResponse>> Add(AddClientRequest request)
    {
        var client = await Mediator.Send(new AddClientCommand(GetUserId(), request));
        return CreatedAtAction(nameof(Get), new { id = client.Id }, client);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(NoContentResult), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> Update(string id, UpdateClientRequest request)
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
    [ProducesResponseType(typeof(NoContentResult), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> Deactivate(string id)
    {
        await Mediator.Send(new DeactivateClientCommand(GetUserId(), new DeactivateClientRequest(id)));
        return NoContent();
    }

    [HttpPut("{id}/activate")]
    [ProducesResponseType(typeof(NoContentResult), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> Activate(string id)
    {
        await Mediator.Send(new ActivateClientCommand(GetUserId(), new ActivateClientRequest(id)));
        return NoContent();
    }
}
