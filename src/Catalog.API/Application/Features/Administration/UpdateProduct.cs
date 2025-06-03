using CatalogAPI.Application.Domain.Dtos;
using CatalogAPI.Application.Shared.Validators;
using CatalogAPI.Application.Domain.Interfaces.Repositories;

using Shared.Common.Enums;
using Shared.Common.Exceptions;

using MediatR;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CatalogAPI.Application.Features.Administration;


public record UpdateProductPayload(string Id, string Name, string Description, double Price, ProductType Type, int RestockThreshold, int MaxStockThreshold, List<ProductImagePayload>? DeleteImages, List<ProductImagePayload>? NewImages);

public record UpdateProductCommand(string? TenantId, string? CurrentUserId, UpdateProductPayload Payload)
    : IRequest;


public class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductCommandValidator()
    {
        RuleFor(x => x.Payload)
            .NotNull().WithMessage("Request payload is required.")
            .SetValidator(new UpdateProductPayloadValidator());
    }

    class UpdateProductPayloadValidator : AbstractValidator<UpdateProductPayload>
    {
        public UpdateProductPayloadValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Product Id is required.");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required.")
                .MaximumLength(100).WithMessage("Name cannot exceed 100 characters.");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Description is required.")
                .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters.");

            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("Price must be greater than zero.")
                .LessThan(1000).WithMessage("Price must be less than a thousand.");

            RuleFor(x => x.Type)
                .IsInEnum().WithMessage("Invalid product type.");

            RuleFor(x => x.RestockThreshold)
                .GreaterThanOrEqualTo(5).WithMessage("Restock threshold must be greater than or equal 5.");

            RuleFor(x => x.MaxStockThreshold)
                .GreaterThanOrEqualTo(10).WithMessage("Maximum stock threshold must be greater than or equal 10")
                .LessThanOrEqualTo(1000).WithMessage("Maximum stock threshold must be less than or equal 1000.");

            RuleForEach(x => x.NewImages)
                .SetValidator(new ProductImagePayloadValidator())
                .When(x => x.NewImages != null);

            RuleForEach(x => x.DeleteImages)
                .SetValidator(new ProductImagePayloadValidator())
                .When(x => x.DeleteImages != null);
        }
    }
}


internal sealed class UpdateProductCommandHandler(ILogger<UpdateProductCommandHandler> logger, IProductRepository productRepository) : IRequestHandler<UpdateProductCommand>
{
    private readonly ILogger<UpdateProductCommandHandler> _logger = logger;
    private readonly IProductRepository _productRepository = productRepository;

    public async Task Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling request update product: {@Request}", request);

        // TODO: refactor to reuse this validation
        if (string.IsNullOrWhiteSpace(request.TenantId) || string.IsNullOrWhiteSpace(request.CurrentUserId))
            throw new UnauthorizedAccessException("Invalid token.");

        var requestPayload = request.Payload;
        var product = await _productRepository.GetByIdAsync(requestPayload.Id, cancellationToken);
        if (product == null)
            throw new NotFoundException($"Product with id: {requestPayload.Id} not found.");
            
        /* Perform update */
        product.UpdateDetails(
            requestPayload.Name,
            requestPayload.Description,
            requestPayload.Price,
            requestPayload.Type,
            requestPayload.RestockThreshold,
            requestPayload.MaxStockThreshold,
            request.CurrentUserId);

        if (requestPayload.NewImages != null && requestPayload.NewImages.Any())
        {
            var newPrimaryImage = requestPayload.NewImages.SingleOrDefault(ni => ni.IsPrimary);
            if (newPrimaryImage != null)
                product.UpdatePrimaryImage(newPrimaryImage);

            product.AddImages([.. requestPayload.NewImages.Where(ni => !ni.IsPrimary)]);
        }

        if (requestPayload.DeleteImages != null && requestPayload.DeleteImages.Any())
            product.RemoveImages([..requestPayload.DeleteImages.Where(ni => !ni.IsPrimary)]);

        _logger.LogInformation("Updating product to database: {@UpdatedProduct}", product);
        
        _productRepository.Update(product);
        await _productRepository.SaveChangesAsync(cancellationToken);
    }
}