using CatalogAPI.Application.Common.Exceptions;
using CatalogAPI.Application.Domain.Catalogs;
using CatalogAPI.Application.Features.Products.Shared.Common;
using CatalogAPI.Application.Features.Products.Shared.Dtos;
using CatalogAPI.Application.Features.Products.Shared.Validators;
using CatalogAPI.Application.Infrastructure.Persistence;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ValidationException = CatalogAPI.Application.Common.Exceptions.ValidationException;

namespace CatalogAPI.Application.Features.Products;


public record UpdateProductCommand(string Id, string Name, string Description, decimal Price, ProductType Type, ProductStatus Status, int AvailableStock, int RestockThreshold, int MaxStockThreshold, List<ProductImageRequest>? DeleteImages, List<ProductImageRequest>? NewImages)
    : IRequest;


public class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductCommandValidator()
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
            .SetValidator(new ProductImageRequestValidator())
            .When(x => x.NewImages != null);

        RuleForEach(x => x.DeleteImages)
            .SetValidator(new ProductImageRequestValidator())
            .When(x => x.DeleteImages != null);
    }

    private bool NotExceedPrimaryImageQuantity(List<ProductImageRequest>? images) 
        => images?.Where(x => x.IsPrimary).Count() <= 1;
}


internal sealed class UpdateProductCommandHandler(ApplicationDbContext context) : IRequestHandler<UpdateProductCommand>
{
    private readonly ApplicationDbContext _context = context;

    public async Task Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _context.Products
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);
        if (product == null) 
            throw new NotFoundException($"Product with id: {request.Id} not found.");

        /* Check if product has primary image */
        var deleteImages = Enumerable.Empty<Image>();
        if (request.DeleteImages != null && request.DeleteImages.Any()) 
        {
            deleteImages = product.Images.Where(i => request.DeleteImages.Any(di => di.URL == i.URL));
        }

        var newImages = Enumerable.Empty<Image>();
        if (request.NewImages != null && request.NewImages.Any())
        {
            newImages = request.NewImages.Select(ToEntity);
        }

        var newPrimaryImage = newImages.FirstOrDefault(ni => ni.IsPrimary);
        var deletePrimaryImage = deleteImages.FirstOrDefault(di => di.IsPrimary);
        if (deletePrimaryImage != null && newPrimaryImage == null)
        {
            throw new ValidationException("Missing primary product image.");
        }

        /* Check if images exceed maximum quantity */
        var count = product.Images.Count() - deleteImages.Count() + newImages.Count();
        if (count > ProductConstants.MAXIMUM_IMAGE_QUANTITY) 
            throw new ValidationException("Exceeding maximum image quantity.");

        /* Perform update */
        product.Name = request.Name;
        product.Description = request.Description;
        product.Price = request.Price;
        product.Type = request.Type;
        product.Status = request.Status;
        product.AvailableStock = request.AvailableStock;
        product.RestockThreshold = request.RestockThreshold;
        product.MaxStockThreshold = request.MaxStockThreshold;

        if (deleteImages.Any())
            product.Images = product.Images.Where(i => !deleteImages.Contains(i)).ToList();

        if (newImages.Any())
            product.Images.AddRange(newImages);

        _context.Update(product);
        
        await _context.SaveChangesAsync(cancellationToken);
    }

    private static Image ToEntity(ProductImageRequest image) => new()
    {
        Id = Guid.NewGuid().ToString(),
        Filename = image.Filename,
        URL = image.URL,
        AltText = image.AltText,
        Size = image.Size,
        IsPrimary = image.IsPrimary
    };
}