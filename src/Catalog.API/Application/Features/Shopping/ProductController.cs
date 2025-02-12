using CatalogAPI.Application.Shared.Dtos;

using Shared.Common;
using Shared.Common.Models;

using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CatalogAPI.Application.Features.Shopping;

[ApiController]
[ApiVersion(1)]
[Route("v{v:apiVersion}/api/product-shoppings")]
public class ProductController : ApiControllerBase
{
    [HttpPost("query")]
    [MapToApiVersion(1)]
    [ProducesResponseType(typeof(PaginatedList<ProductBriefResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<PaginatedList<ProductBriefResponse>> Query(GetProductsQuery query)
    {
        return await Mediator.Send(query);
    }

    [HttpGet("{id}", Name = "GetProductDetails")]
    [MapToApiVersion(1)]
    [ProducesResponseType(typeof(ProductDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ProductDetailResponse> Get(string id)
    {
        return await Mediator.Send(new GetProductDetailQuery(id));
    }
}