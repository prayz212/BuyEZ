using CatalogAPI.Application.Common.Exceptions;
using CatalogAPI.Application.Domain.Catalogs;
using CatalogAPI.Application.Features.Products.Shared.Dtos;
using CatalogAPI.Application.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CatalogAPI.Application.Features.Products;


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

        return Product.ToDto(product);
    }

}