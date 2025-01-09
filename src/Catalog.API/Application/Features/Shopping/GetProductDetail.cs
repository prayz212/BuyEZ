using CatalogAPI.Application.Domain;
using CatalogAPI.Application.Shared.Dtos;
using CatalogAPI.Application.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Common.Exceptions;

namespace CatalogAPI.Application.Features.Shopping;


public record GetProductDetailQuery(string Id) : IRequest<ProductDetailResponse>;


internal sealed class GetProductDetailQueryHandler(ApplicationDbContext context)
    : IRequestHandler<GetProductDetailQuery, ProductDetailResponse>
{
    private readonly ApplicationDbContext _context = context;

    public async Task<ProductDetailResponse> Handle(GetProductDetailQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Id)) 
            throw new ValidationException("Invalid product id.");

        var product = await _context.Products
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == request.Id);

        if (product is null)
            throw new NotFoundException($"Product with id: {request.Id} was not found.");

        return Product.ToDto(product);
    }

}