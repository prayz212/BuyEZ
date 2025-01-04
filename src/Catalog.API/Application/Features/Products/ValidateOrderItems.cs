using CatalogAPI.Application.Infrastructure.Persistence;

using Shared.GrpcProto.Catalog;

using MediatR;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace CatalogAPI.Application.Features.Products;

public record ValidateOrderItemsCommand(ValidateOrderItemsRequest Payload) : IRequest<ValidateOrderItemsResponse>;


public class ValidateOrderItemsCommandValidator : AbstractValidator<ValidateOrderItemsCommand>
{
    public ValidateOrderItemsCommandValidator()
    {
        RuleFor(x => x.Payload)
            .NotNull().WithMessage("Request Payload is required.")
            .SetValidator(new ValidateOrderItemsRequestValidator());
    }

    class ValidateOrderItemsRequestValidator : AbstractValidator<ValidateOrderItemsRequest>
    {
        public ValidateOrderItemsRequestValidator()
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


internal sealed class ValidateOrderItemsCommandHandler(ApplicationDbContext applicationDbContext) : IRequestHandler<ValidateOrderItemsCommand, ValidateOrderItemsResponse>
{
    private readonly ApplicationDbContext _applicationDbContext = applicationDbContext;

    public async Task<ValidateOrderItemsResponse> Handle(ValidateOrderItemsCommand request, CancellationToken cancellationToken)
    {
        var ids = request.Payload.Ids;
        var products = await _applicationDbContext.Products
            .Where(p => ids.Contains(p.Id))
            .Select(p => new ProductReference
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                AvailableStock = p.AvailableStock
            })
            .ToListAsync();

        return new ValidateOrderItemsResponse
        {
            Products = products,
            IsEnough = ids.Count == products.Count
        };
    }
}