using CatalogAPI.Application.Domain;
using CatalogAPI.Application.Shared.Dtos;
using CatalogAPI.Application.Shared.Validators;
using CatalogAPI.Application.Domain.Interfaces.Repositories;

using Shared.Common.Enums;

using MediatR;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace CatalogAPI.Application.Features.Administration;


public record AddProductPayload(string Name, string Description, double Price, ProductType Type, List<ProductImagePayload> Images);

public record AddProductCommand(string? TenantId, string? CurrentUserId, AddProductPayload Payload) : IRequest<ProductDetailResponse>;


public class AddProductCommandValidator : AbstractValidator<AddProductCommand>
{
    public AddProductCommandValidator() 
    {
        RuleFor(x => x.Payload)
            .NotNull().WithMessage("Request payload is required.")
            .SetValidator(new AddProductPayloadValidator());
    }

    class AddProductPayloadValidator : AbstractValidator<AddProductPayload>
    {
        public AddProductPayloadValidator() 
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required.")
                .MaximumLength(100).WithMessage("Name cannot exceed 100 characters.");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Description is required.")
                .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters.");

            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("Price must be greater than 0.")
                .LessThan(1000).WithMessage("Price must be less than 1000.");

            RuleFor(x => x.Type)
                .IsInEnum().WithMessage("Invalid product type.");

            RuleForEach(x => x.Images)
                .SetValidator(new ProductImagePayloadValidator());
        }
    }
}


internal sealed class AddProductCommandHandler(ILogger<AddProductCommandHandler> logger, IProductRepository productRepository)
    : IRequestHandler<AddProductCommand, ProductDetailResponse>
{
    private readonly ILogger<AddProductCommandHandler> _logger = logger;
    private readonly IProductRepository _productRepository = productRepository;

    public async Task<ProductDetailResponse> Handle(AddProductCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling request add product: {@Request}", request);
        
        // TODO: refactor to reuse this validation
        if (string.IsNullOrWhiteSpace(request.TenantId) || string.IsNullOrWhiteSpace(request.CurrentUserId))
            throw new UnauthorizedAccessException("Invalid token.");

        var requestPayload = request.Payload;
        var newProduct = Product.CreateNew(
            requestPayload.Name,
            requestPayload.Description,
            requestPayload.Price,
            requestPayload.Type,
            requestPayload.Images.FirstOrDefault(i => i.IsPrimary),
            request.TenantId,
            request.CurrentUserId);
        newProduct.AddImages([.. requestPayload.Images.Where(i => !i.IsPrimary)]);

        _logger.LogInformation("Adding product to database: {@NewProduct}", newProduct);
        await _productRepository.AddAsync(newProduct, cancellationToken);
        await _productRepository.SaveChangesAsync(cancellationToken);

        return newProduct.ToDto();
    }
}