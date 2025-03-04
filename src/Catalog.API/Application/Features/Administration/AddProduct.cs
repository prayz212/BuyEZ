using CatalogAPI.Application.Domain;
using CatalogAPI.Application.Shared.Common;
using CatalogAPI.Application.Shared.Dtos;
using CatalogAPI.Application.Shared.Validators;
using CatalogAPI.Application.Infrastructure.Persistence;

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

            RuleFor(x => x.Images)
                .Must(NotExceedImageQuantity).WithMessage($"Only allowed maximum {ProductConstants.MAXIMUM_IMAGE_QUANTITY} images.")
                .Must(HaveOnlyOnePrimaryImage).WithMessage("Missing or exceeding required primary image quantity.");

            RuleForEach(x => x.Images)
                .SetValidator(new ProductImagePayloadValidator());
        }

        private bool HaveOnlyOnePrimaryImage(List<ProductImagePayload> images) 
            => images.Where(x => x.IsPrimary).Count() == 1;

        private bool NotExceedImageQuantity(List<ProductImagePayload> images)
            => images.Count <= ProductConstants.MAXIMUM_IMAGE_QUANTITY;
    }
}


internal sealed class AddProductCommandHandler(ILogger<AddProductCommandHandler> logger, ApplicationDbContext context)
    : IRequestHandler<AddProductCommand, ProductDetailResponse>
{
    private readonly ILogger<AddProductCommandHandler> _logger = logger;
    private readonly ApplicationDbContext _context = context;

    public async Task<ProductDetailResponse> Handle(AddProductCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling request add product: {@Request}", request);
        
        // TODO: refactor to reuse this validation
        if (string.IsNullOrWhiteSpace(request.TenantId) || string.IsNullOrWhiteSpace(request.CurrentUserId))
            throw new UnauthorizedAccessException("Invalid token.");

        var requestPayload = request.Payload;
        var newProduct = ToEntity(request.TenantId, request.CurrentUserId, requestPayload);
        var newProductImages = requestPayload.Images.Select(i => ToEntity(request.CurrentUserId, i));
        newProduct.Images = newProductImages.ToList();

        _logger.LogInformation("Adding product to database: {@NewProduct}", newProduct);
        await _context.Products.AddAsync(newProduct, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return Product.ToDto(newProduct);
    }

    private static Product ToEntity(string tenantId, string createdBy, AddProductPayload product) => new()
    {
        Id = Guid.NewGuid().ToString(),
        Name = product.Name,
        Description = product.Description,
        Price = product.Price,
        Type = product.Type,
        TenantId = tenantId,
        CreatedBy = createdBy
    };

    private static Image ToEntity(string createdBy, ProductImagePayload image) => new()
    {
        Id = Guid.NewGuid().ToString(),
        Filename = image.Filename,
        URL = image.URL,
        AltText = image.AltText,
        Size = image.Size,
        IsPrimary = image.IsPrimary,
        CreatedBy = createdBy
    };
}