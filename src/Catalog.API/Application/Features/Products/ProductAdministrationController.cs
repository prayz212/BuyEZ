using CatalogAPI.Application.Common;
using CatalogAPI.Application.Common.Constants;
using CatalogAPI.Application.Common.Exceptions;
using CatalogAPI.Application.Features.Products;
using CatalogAPI.Application.Features.Products.Shared.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CatalogAPI.Application.Features;

[ApiController]
[Route($"{ApiPaths.Root}/product-administrations")]
public class ProductAdministrationController : ApiControllerBase
{
    [HttpPost]
    [Authorize(PolicyConstants.TENANT_ADMIN_OR_MANAGER_POLICY)]
    [ProducesResponseType(typeof(ProductDetailResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ProductDetailResponse>> Add(AddProductRequest request)
    {
        var product = await Mediator.Send(new AddProductCommand(GetTenantId(), GetUserId(), request));
        return CreatedAtRoute("GetProductDetails", new { id = product.Id}, product);
    }

    [HttpPut("{id}")]
    [Authorize(PolicyConstants.TENANT_ADMIN_OR_MANAGER_POLICY)]
    [ProducesResponseType(typeof(NoContentResult), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> Update(string id, UpdateProductRequest request)
    {
        if (id != request.Id) 
            throw new ValidationException("Product Id is not correct.");

        await Mediator.Send(new UpdateProductCommand(GetTenantId(), GetUserId(), request));
        return NoContent();
    }
}