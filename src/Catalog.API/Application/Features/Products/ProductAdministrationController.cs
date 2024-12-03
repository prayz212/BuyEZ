using CatalogAPI.Application.Common;
using CatalogAPI.Application.Common.Exceptions;
using CatalogAPI.Application.Features.Products;
using CatalogAPI.Application.Features.Products.Shared.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CatalogAPI.Application.Features;

[ApiController]
[Route($"{ApiPaths.Root}/product-administrations")]
public class ProductAdministrationController : ApiControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(ProductDetailResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ProductDetailResponse>> Add(AddProductCommand command)
    {
        var product = await Mediator.Send(command);
        return CreatedAtRoute("GetProductDetails", new { id = product.Id}, product);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(NoContentResult), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> Update(string id, UpdateProductCommand command)
    {
        if (id != command.Id) 
            throw new ValidationException("Product Id is not correct.");

        await Mediator.Send(command);
        return NoContent();
    }
}