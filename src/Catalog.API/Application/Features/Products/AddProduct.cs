using CatalogAPI.Application.Domain.Catalogs;
using CatalogAPI.Application.Features.Products.Shared.Common;
using CatalogAPI.Application.Features.Products.Shared.Dtos;
using CatalogAPI.Application.Features.Products.Shared.Validators;
using CatalogAPI.Application.Infrastructure.Persistence;
using FluentValidation;
using MediatR;

namespace CatalogAPI.Application.Features.Products;


public record AddProductCommand(string Name, string Description, decimal Price, ProductType Type, List<ProductImageRequest> Images)
    : IRequest<ProductDetailResponse>;


public class AddProductCommandValidator : AbstractValidator<AddProductCommand>
{
    public AddProductCommandValidator() 
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

        RuleFor(x => x.Images)
            .Must(NotExceedImageQuantity).WithMessage($"Only allowed maximum {ProductConstants.MAXIMUM_IMAGE_QUANTITY} images.")
            .Must(HaveOnlyOnePrimaryImage).WithMessage("Missing or exceeding required primary image quantity.");

        RuleForEach(x => x.Images)
            .SetValidator(new ProductImageRequestValidator());
    }

    private bool HaveOnlyOnePrimaryImage(List<ProductImageRequest> images) 
        => images.Where(x => x.IsPrimary).Count() == 1;

    private bool NotExceedImageQuantity(List<ProductImageRequest> images)
        => images.Count <= ProductConstants.MAXIMUM_IMAGE_QUANTITY;
}


internal sealed class AddProductCommandHandler(ApplicationDbContext context)
    : IRequestHandler<AddProductCommand, ProductDetailResponse>
{
    private readonly ApplicationDbContext _context = context;

    public async Task<ProductDetailResponse> Handle(AddProductCommand request, CancellationToken cancellationToken)
    {
        var newProduct = ToEntity(request);
        var newProductImages = request.Images.Select(i => ToEntity(i));
        newProduct.Images = newProductImages.ToList();

        await _context.Products.AddAsync(newProduct, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return Product.ToDto(newProduct);
    }

    private static Product ToEntity(AddProductCommand product) => new()
    {
        Id = Guid.NewGuid().ToString(),
        Name = product.Name,
        Description = product.Description,
        Price = product.Price,
        Type = product.Type
    };

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