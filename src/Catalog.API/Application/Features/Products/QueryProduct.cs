using CatalogAPI.Application.Common;
using CatalogAPI.Application.Common.Mappings;
using CatalogAPI.Application.Common.Models;
using CatalogAPI.Application.Domain.Catalogs;
using CatalogAPI.Application.Infrastructure.Persistence;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CatalogAPI.Application.Features.Products;

[ApiController]
[Route($"{ApiPaths.Root}/products")]
public class QueryProductController : ApiControllerBase
{
    [HttpPost("query")]
    public async Task<PaginatedList<ProductBriefResponse>> Query(QueryProductRequest query)
    {
        return await Mediator.Send(query);
    }
}

public record ImageBriefResponse(string Filename, string Url, string AltText);
public record ProductBriefResponse(string Id, string Name, decimal Price, ProductType Type, ImageBriefResponse? Image);

public record QueryProductRequest(int PageNumber = 1, int PageSize = 10) 
    : IRequest<PaginatedList<ProductBriefResponse>>;

public class QueryProductRequestValidator : AbstractValidator<QueryProductRequest>
{
    public QueryProductRequestValidator()
    {
        RuleFor(x => x.PageNumber)
            .NotEmpty().WithMessage("PageNumber is required.")
            .GreaterThanOrEqualTo(1).WithMessage("PageNumber at least greater than or equal to 1.");

        RuleFor(x => x.PageSize)
            .NotEmpty().WithMessage("PageSize is required.")
            .GreaterThanOrEqualTo(1).WithMessage("PageSize at least greater than or equal to 1.");
    }
}

internal sealed class QueryProductRequestHandler(ApplicationDbContext context) 
    : IRequestHandler<QueryProductRequest, PaginatedList<ProductBriefResponse>>
{
    private readonly ApplicationDbContext _context = context;

    public async Task<PaginatedList<ProductBriefResponse>> Handle(QueryProductRequest request, CancellationToken cancellationToken)
    {
        return await _context.Products
            .Include(p => p.Images!.Where(i => i.IsPrimary))
            .OrderBy(p => p.Created)
            .Select(p => ToDto(p))
            .PaginatedListAsync(request.PageNumber, request.PageSize, cancellationToken);
    }

    private static ProductBriefResponse ToDto(Product product) =>
        new(product.Id, product.Name, product.Price, product.ProductType, ToDto(product.Images?.FirstOrDefault()));

    private static ImageBriefResponse? ToDto(Image? image) => 
        image is not null ? new(image.Filename, image.URL, image.AltText) : null;
}

/* 
UI: we will have multiple stages to create a product:
- Input product details => this api call
- Add product images => in the next api call (images api)
*/
