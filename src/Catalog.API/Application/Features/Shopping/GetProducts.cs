using CatalogAPI.Application.Domain;
using CatalogAPI.Application.Infrastructure.Persistence;

using Shared.Common.Enums;
using Shared.Common.Models;
using Shared.Common.Mappings;

using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CatalogAPI.Application.Features.Shopping;


public record ImageBriefResponse(string Filename, string Url, string AltText);
public record ProductBriefResponse(string Id, string Name, double Price, ProductType Type, ImageBriefResponse? Image);

public record GetProductsQuery(int PageNumber = 1, int PageSize = 10) 
    : IRequest<PaginatedList<ProductBriefResponse>>;


public class GetProductsQueryValidator : AbstractValidator<GetProductsQuery>
{
    public GetProductsQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .NotEmpty().WithMessage("PageNumber is required.")
            .GreaterThanOrEqualTo(1).WithMessage("PageNumber at least greater than or equal to 1.");

        RuleFor(x => x.PageSize)
            .NotEmpty().WithMessage("PageSize is required.")
            .GreaterThanOrEqualTo(1).WithMessage("PageSize at least greater than or equal to 1.");
    }
}


internal sealed class GetProductsQueryHandler(ApplicationDbContext context) 
    : IRequestHandler<GetProductsQuery, PaginatedList<ProductBriefResponse>>
{
    private readonly ApplicationDbContext _context = context;

    public async Task<PaginatedList<ProductBriefResponse>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        return await _context.Products
            .Include(p => p.Images.Where(i => i.IsPrimary))
            .OrderBy(p => p.Created)
            .Select(p => ToDto(p))
            .PaginatedListAsync(request.PageNumber, request.PageSize, cancellationToken);
    }

    private static ProductBriefResponse ToDto(Product product) =>
        new(product.Id, product.Name, product.Price, product.Type, ToDto(product.Images.FirstOrDefault()));

    private static ImageBriefResponse? ToDto(Image? image) => 
        image is not null ? new(image.Filename, image.URL, image.AltText) : null;
}