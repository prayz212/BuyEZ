using CatalogAPI.Application.Common;
using CatalogAPI.Application.Common.Exceptions;
using CatalogAPI.Application.Domain.Catalogs;
using CatalogAPI.Application.Infrastructure.Persistence;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CatalogAPI.Application.Features.Products;

[ApiController]
[Route($"{ApiPaths.Root}/products")]
public class GetProductController : ApiControllerBase
{
    [HttpGet("{id}")]
    public async Task<ProductDetailResponse> Get(string id)
    {
        return await Mediator.Send(new GetProductRequest(id));
    }
}

public record ImageDetailResponse(string Filename, string Url, string AltText, bool IsPrimary, int Size);
public record ProductDetailResponse(string Id, string Name, string Description, decimal Price, ProductType Type, ProductStatus Status, List<ImageDetailResponse>? Images);

public record GetProductRequest(string Id) : IRequest<ProductDetailResponse>;

internal sealed class GetProductRequestHandler(ApplicationDbContext context)
    : IRequestHandler<GetProductRequest, ProductDetailResponse>
{
    private readonly ApplicationDbContext _context = context;

    public async Task<ProductDetailResponse> Handle(GetProductRequest request, CancellationToken cancellationToken)
    {
        var product = await _context.Products
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == request.Id);

        if (product is null)
            throw new NotFoundException($"Product with id: {request.Id} was not found.");

        return ToDto(product);
    }

    private static ProductDetailResponse ToDto(Product product) =>
        new(product.Id, product.Name, product.Description, product.Price, product.ProductType, product.Status, product.Images?.Select(ToDto).ToList());

    private static ImageDetailResponse ToDto(Image image) =>
        new(image.Filename, image.URL, image.AltText, image.IsPrimary, image.Size);
}