using OrderAPI.Application.Shared.Dtos;

using Shared.Common;
using Shared.Common.Models;
using Shared.Common.Constants;
using Shared.Common.Exceptions;

using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace OrderAPI.Application.Features.Shopping;

[ApiController]
[ApiVersion(1)]
[Route($"{ApiPaths.Root}/order-shoppings")]
[Authorize(Policy = PolicyConstants.CUSTOMER_POLICY)]
public class OrderController : ApiControllerBase
{
    [HttpGet("{id}")]
    [MapToApiVersion(1)]
    [ProducesResponseType(typeof(OrderDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<OrderDetailResponse> Get(string id)
    {
        return await Mediator.Send(new GetOrderDetailQuery(GetUserId(), id));
    }

    [HttpPost("query")]
    [MapToApiVersion(1)]
    [ProducesResponseType(typeof(PaginatedList<OrderBriefResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<PaginatedList<OrderBriefResponse>> Query(GetOrdersPayload payload)
    {
        return await Mediator.Send(new GetOrdersQuery(GetUserId(), payload));
    }

    [HttpPost]
    [MapToApiVersion(1)]
    [ProducesResponseType(typeof(OrderDetailResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<OrderDetailResponse>> Add(AddOrderPayload request)
    {
        var order = await Mediator.Send(new AddOrderCommand(GetUserId(), request));
        return CreatedAtAction(nameof(Get), new { id = order.Id }, order);
    }

    [HttpPut("{id}")]
    [MapToApiVersion(1)]
    [ProducesResponseType(typeof(NoContentResult), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> Update(string id, UpdateOrderPayload request)
    {
        if (id != request.Id) 
            throw new ValidationException("Order Id is not correct.");
            
        await Mediator.Send(new UpdateOrderCommand(GetUserId(), request));

        return NoContent();
    }

    [HttpPost("{id}/cancel")]
    [MapToApiVersion(1)]
    [ProducesResponseType(typeof(NoContentResult), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Cancel(string id)
    {
        await Mediator.Send(new CancelOrderCommand(GetUserId(), id));

        return NoContent();
    }
}