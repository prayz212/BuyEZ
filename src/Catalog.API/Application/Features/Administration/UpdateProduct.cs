using CatalogAPI.Application.Domain;
using CatalogAPI.Application.Shared.Common;
using CatalogAPI.Application.Shared.Dtos;
using CatalogAPI.Application.Shared.Validators;
using CatalogAPI.Application.Infrastructure.Persistence;

using Shared.Common.Enums;
using Shared.Common.Exceptions;
using ValidationException = Shared.Common.Exceptions.ValidationException;

using MediatR;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace CatalogAPI.Application.Features.Administration;


public record UpdateProductPayload(string Id, string Name, string Description, double Price, ProductType Type, ProductStatus Status, int AvailableStock, int RestockThreshold, int MaxStockThreshold, List<ProductImagePayload>? DeleteImages, List<ProductImagePayload>? NewImages);

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

            RuleFor(x => x.Status)
                .IsInEnum().WithMessage("Invalid product status.");

            RuleFor(x => x.AvailableStock)
                .GreaterThanOrEqualTo(0).WithMessage("Available stock must be greater than or equal 0.");

            RuleFor(x => x.RestockThreshold)
                .GreaterThanOrEqualTo(5).WithMessage("Restock threshold must be greater than or equal 5.");

            RuleFor(x => x.MaxStockThreshold)
                .GreaterThanOrEqualTo(10).WithMessage("Maximum stock threshold must be greater than or equal 10")
                .LessThanOrEqualTo(1000).WithMessage("Maximum stock threshold must be less than or equal 1000.");

            RuleFor(x => x.NewImages)
                .Must(NotExceedPrimaryImageQuantity).WithMessage("Exceeding required primary image quantity.")
                .When(x => x.NewImages != null);

            RuleForEach(x => x.NewImages)
                .SetValidator(new ProductImagePayloadValidator())
                .When(x => x.NewImages != null);

            RuleForEach(x => x.DeleteImages)
                .SetValidator(new ProductImagePayloadValidator())
                .When(x => x.DeleteImages != null);
        }

        private bool NotExceedPrimaryImageQuantity(List<ProductImagePayload>? images) 
            => images?.Where(x => x.IsPrimary).Count() <= 1;
    }
}


internal sealed class UpdateProductCommandHandler(ApplicationDbContext context) : IRequestHandler<UpdateProductCommand>
{
    private readonly ApplicationDbContext _context = context;

    public async Task Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        // TODO: refactor to reuse this validation
        if (string.IsNullOrWhiteSpace(request.TenantId) || string.IsNullOrWhiteSpace(request.CurrentUserId))
            throw new UnauthorizedAccessException("Invalid token.");

        var requestPayload = request.Payload;
        var product = await _context.Products
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == requestPayload.Id && p.TenantId == request.TenantId, cancellationToken);
        if (product == null) 
            throw new NotFoundException($"Product with id: {requestPayload.Id} not found.");

        /* Check if product has primary image */
        var deleteImages = Enumerable.Empty<Image>();
        if (requestPayload.DeleteImages != null && requestPayload.DeleteImages.Any()) 
        {
            deleteImages = product.Images.Where(i => requestPayload.DeleteImages.Any(di => di.URL == i.URL));
        }

        var newImages = Enumerable.Empty<Image>();
        if (requestPayload.NewImages != null && requestPayload.NewImages.Any())
        {
            newImages = requestPayload.NewImages.Select(ni => ToEntity(request.CurrentUserId, ni));
        }

        var newPrimaryImage = newImages.FirstOrDefault(ni => ni.IsPrimary);
        var deletePrimaryImage = deleteImages.FirstOrDefault(di => di.IsPrimary);
        var isMissingPrimaryImage = deletePrimaryImage != null && newPrimaryImage == null;
        var isExceedingPrimaryImage = deletePrimaryImage == null && newPrimaryImage != null;
        if (isMissingPrimaryImage || isExceedingPrimaryImage)
        {
            throw new ValidationException("Missing or exceeding required primary image quantity.");
        }

        /* Check if images exceed maximum quantity */
        var count = product.Images.Count() - deleteImages.Count() + newImages.Count();
        if (count > ProductConstants.MAXIMUM_IMAGE_QUANTITY) 
            throw new ValidationException("Exceeding maximum image quantity.");

        /* Perform update */
        product.Name = requestPayload.Name;
        product.Description = requestPayload.Description;
        product.Price = requestPayload.Price;
        product.Type = requestPayload.Type;
        product.Status = requestPayload.Status;
        product.AvailableStock = requestPayload.AvailableStock;
        product.RestockThreshold = requestPayload.RestockThreshold;
        product.MaxStockThreshold = requestPayload.MaxStockThreshold;
        product.LastModifiedBy = request.CurrentUserId;

        if (deleteImages.Any())
            product.Images = product.Images.Where(i => !deleteImages.Contains(i)).ToList();

        if (newImages.Any())
            product.Images.AddRange(newImages);

        _context.Update(product);
        
        await _context.SaveChangesAsync(cancellationToken);
    }

    private static Image ToEntity(string modifiedBy, ProductImagePayload image) => new()
    {
        Id = Guid.NewGuid().ToString(),
        Filename = image.Filename,
        URL = image.URL,
        AltText = image.AltText,
        Size = image.Size,
        IsPrimary = image.IsPrimary,
        LastModifiedBy = modifiedBy
    };
}