using CatalogAPI.Application.Common;
using CatalogAPI.Application.Common.Exceptions;
using CatalogAPI.Application.Common.Models;
using CatalogAPI.Application.Features.Products;
using CatalogAPI.Application.Features.Products.Shared.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CatalogAPI.Application.Features;

[ApiController]
[Route($"{ApiPaths.Root}/products")]
public class ProductController : ApiControllerBase
{
    [HttpPost("query")]
    [ProducesResponseType(typeof(PaginatedList<ProductBriefResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<PaginatedList<ProductBriefResponse>> Query(QueryProductRequest query)
    {
        return await Mediator.Send(query);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ProductDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ProductDetailResponse> Get(string id)
    {
        return await Mediator.Send(new GetProductRequest(id));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ProductDetailResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ProductDetailResponse>> AddProduct(AddProductCommand command)
    {
        var product = await Mediator.Send(command);
        return CreatedAtAction(nameof(Get), new { id = product.Id}, product);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(NoContentResult), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> UpdateProduct(string id, UpdateProductCommand command)
    {
        if (id != command.Id) 
            throw new ValidationException("Product Id is not correct.");

        await Mediator.Send(command);
        return NoContent();
    }
}