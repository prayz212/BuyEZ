using CatalogAPI.Application.Infrastructure.Persistence;

using Shared.GrpcProto.Catalog;

using MediatR;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace CatalogAPI.Application.Features.Shopping.gRPC;


public record GetOrderItemsQuery(GetOrderItemsPayload Payload) : IRequest<GetOrderItemsResponse>;


public class GetOrderItemsQueryValidator : AbstractValidator<GetOrderItemsQuery>
{
    public GetOrderItemsQueryValidator()
    {
        RuleFor(x => x.Payload)
            .NotNull().WithMessage("Request Payload is required.")
            .SetValidator(new GetOrderItemsPayloadValidator());
    }

    class GetOrderItemsPayloadValidator : AbstractValidator<GetOrderItemsPayload>
    {
        public GetOrderItemsPayloadValidator()
        {
            RuleFor(x => x.Ids)
                .NotEmpty().WithMessage("Product IDs is required.");

            RuleForEach(x => x.Ids)
                .Cascade(CascadeMode.Stop)
                .Must(BeAValidGuid).WithMessage("Product Id is not valid.");
        }

        private bool BeAValidGuid(string id)
        {
            return Guid.TryParse(id, out _);
        }
    }
}


internal sealed class GetOrderItemsQueryHandler(ApplicationDbContext context) : IRequestHandler<GetOrderItemsQuery, GetOrderItemsResponse>
{
    private readonly ApplicationDbContext _context = context;

    public async Task<GetOrderItemsResponse> Handle(GetOrderItemsQuery request, CancellationToken cancellationToken)
    {
        var ids = request.Payload.Ids;
        var products = await _context.Products
            .Where(p => ids.Contains(p.Id))
            .Select(p => new ProductReference
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                AvailableStock = p.AvailableStock
            })
            .ToListAsync();

        return new GetOrderItemsResponse
        {
            Products = products,
            IsEnough = ids.Count == products.Count
        };
    }
}