using ClientManagementAPI.Application.Common;
using ClientManagementAPI.Application.Common.Exceptions;
using ClientManagementAPI.Application.Common.Models;
using ClientManagementAPI.Application.Features.Clients.Shared.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ClientManagementAPI.Application.Features.Clients;

[ApiController]
[Route($"{ApiPaths.Root}/clients")]
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
    public async Task<ActionResult<ClientDetailResponse>> Add(AddClientCommand command)
    {
        var client = await Mediator.Send(command);
        return CreatedAtAction(nameof(Get), new { id = client.Id }, client);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(NoContentResult), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> Update(string id, UpdateClientCommand command)
    {
        if (id != command.Id) 
            throw new ValidationException("Client Id is not correct.");

        await Mediator.Send(command);

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
        await Mediator.Send(new DeactivateClientCommand(id));
        return NoContent();
    }

    [HttpPut("{id}/activate")]
    [ProducesResponseType(typeof(NoContentResult), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> Activate(string id)
    {
        await Mediator.Send(new ActivateClientCommand(id));
        return NoContent();
    }
}
