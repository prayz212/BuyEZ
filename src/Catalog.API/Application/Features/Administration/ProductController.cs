using CatalogAPI.Application.Shared.Dtos;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Shared.Common;
using Shared.Common.Constants;
using Shared.Common.Exceptions;

namespace CatalogAPI.Application.Features.Administration;

[ApiController]
[Route($"{ApiPaths.Root}/product-administrations")]
public class ProductController : ApiControllerBase
{
    [HttpPost]
    [Authorize(PolicyConstants.TENANT_ADMIN_OR_MANAGER_POLICY)]
    [ProducesResponseType(typeof(ProductDetailResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ProductDetailResponse>> Add(AddProductPayload request)
    {
        var product = await Mediator.Send(new AddProductCommand(GetTenantId(), GetUserId(), request));
        return CreatedAtRoute("GetProductDetails", new { id = product.Id}, product);
    }

    [HttpPut("{id}")]
    [Authorize(PolicyConstants.TENANT_ADMIN_OR_MANAGER_POLICY)]
    [ProducesResponseType(typeof(NoContentResult), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> Update(string id, UpdateProductPayload request)
    {
        if (id != request.Id) 
            throw new ValidationException("Product Id is not correct.");

        await Mediator.Send(new UpdateProductCommand(GetTenantId(), GetUserId(), request));
        return NoContent();
    }
}