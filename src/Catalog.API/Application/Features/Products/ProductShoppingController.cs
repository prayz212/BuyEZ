using CatalogAPI.Application.Common;
using CatalogAPI.Application.Common.Models;
using CatalogAPI.Application.Features.Products;
using CatalogAPI.Application.Features.Products.Shared.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CatalogAPI.Application.Features;

[ApiController]
[Route($"{ApiPaths.Root}/product-shoppings")]
public class ProductShoppingController : ApiControllerBase
{
    [HttpPost("query")]
    [ProducesResponseType(typeof(PaginatedList<ProductBriefResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<PaginatedList<ProductBriefResponse>> Query(QueryProductRequest query)
    {
        return await Mediator.Send(query);
    }

    [HttpGet("{id}", Name = "GetProductDetails")]
    [ProducesResponseType(typeof(ProductDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ProductDetailResponse> Get(string id)
    {
        return await Mediator.Send(new GetProductRequest(id));
    }
}