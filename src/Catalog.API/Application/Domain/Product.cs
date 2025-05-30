using CatalogAPI.Application.Domain.Dtos;
using CatalogAPI.Application.Domain.Common;
using CatalogAPI.Application.Domain.Events;

using Shared.Common;
using Shared.Common.Enums;
using Shared.Common.Interfaces;

using FluentValidation;

namespace CatalogAPI.Application.Domain;

public class Product : AuditableEntity, IAggregateRoot
{
    public string Id { get; init; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public double Price { get; private set; } = 0;

    public ProductType Type { get; private set; }

    public int AvailableStock { get; private set; } = 0;

    public int RestockThreshold { get; private set; } = 5;

    public int MaxStockThreshold { get; private set; } = 10;

    public ProductStatus Status { get; private set; } = ProductStatus.OutOfStock;

    public string TenantId { get; private set; } = string.Empty;

    // Navigation property for the related Product
    private readonly List<Image> _images = [];
    public IReadOnlyCollection<Image> Images => _images.AsReadOnly();

    // Domain Events property
    private readonly List<DomainEvent> _domainEvents = [];
    public IReadOnlyList<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    // Constructors
    /*
        - By default, compiler will create a default parameterless constructor.
            => Compiler will NOT generate a parameterless constructor in case we already defined parameters constructor
     
        - Parameterless constructor for EF Core to instantiate the object with the data got from database
     */
    internal Product() { }

    private Product(
        string name,
        string description,
        double price,
        ProductType type,
        string tenantId,
        List<Image> images,
        string createdBy)
    {
        Id = Guid.NewGuid().ToString();
        Name = name;
        Description = description;
        Price = price;
        Type = type;
        Status = ProductStatus.OutOfStock;
        TenantId = tenantId;
        CreatedBy = createdBy;

        _images = images;
    }

    public static Product CreateNew(
        string name,
        string description,
        double price,
        ProductType type,
        ProductImagePayload? primaryImage,
        string tenantId,
        string createdBy)
    {
        if (primaryImage == null || !primaryImage.IsPrimary)
            throw new ValidationException("Required a primary image for product creation.");

        return new Product(
            name,
            description,
            price,
            type,
            tenantId,
            [Image.CreateNew(primaryImage, createdBy)],
            createdBy);
    }

    public void AddToInventory(int quantity)
    {
        if (quantity <= 0)
            throw new ValidationException("Quantity cannot be negative.");

        if (AvailableStock + quantity > MaxStockThreshold)
            throw new ValidationException("Cannot exceeding Maximum Stock threshold.");

        AvailableStock += quantity;

        if (AvailableStock > 0 && Status == ProductStatus.OutOfStock)
            Status = ProductStatus.InStock;
    }

    public void RemoveFromInventory(int quantity)
    {
        if (quantity <= 0)
            throw new ValidationException("Quantity cannot be negative.");

        if (AvailableStock < quantity)
            throw new ValidationException("Do not have enough available product.");

        AvailableStock -= quantity;

        if (AvailableStock == 0)
            Status = ProductStatus.OutOfStock;

        _domainEvents.Add(new RestockThresholdReachedDomainEvent(Id, AvailableStock));
    }

    public void UpdateDetails(
        string name,
        string description,
        double price,
        ProductType type,
        int restockThreshold,
        int maxStockThreshold,
        string modifiedBy)
    {
        Name = name;
        Description = description;
        Price = price;
        Type = type;
        RestockThreshold = restockThreshold;
        MaxStockThreshold = maxStockThreshold;
        LastModifiedBy = modifiedBy;
    }

    public void AddImages(List<ProductImagePayload> images)
    {
        var isExceedAllowedImages = images.Count > ProductConstants.MAXIMUM_IMAGE_QUANTITY
            || _images.Count + images.Count > ProductConstants.MAXIMUM_IMAGE_QUANTITY;
        if (isExceedAllowedImages)
            throw new ValidationException($"Only allowed maximum {ProductConstants.MAXIMUM_IMAGE_QUANTITY} images.");

        var containPrimaryImage = images.Any(i => i.IsPrimary);
        if (containPrimaryImage)
            throw new ValidationException("Missing or exceeding required primary image quantity.");

        foreach (var image in images)
            _images.Add(Image.CreateNew(image, CreatedBy));
    }

    public void RemoveImages(List<ProductImagePayload> images)
    {
        foreach (var image in images)
        {
            if (image.IsPrimary)
                throw new ValidationException("Cannot remove primary image.");

            var imageToRemove = _images.FirstOrDefault(i => i.Filename == image.Filename && i.URL == image.URL);
            if (imageToRemove == null)
                continue;

            _images.Remove(imageToRemove);
        }
    }

    public void UpdatePrimaryImage(ProductImagePayload image)
    {
        if (!image.IsPrimary)
            throw new ValidationException("Required a primary image for product updating.");

        var imageToRemove = _images.First(i => i.IsPrimary);
        _images.Remove(imageToRemove);

        var imageToUpdate = Image.CreateNew(image);
        _images.Add(imageToUpdate);
    }

    public void AddDomainEvent(DomainEvent @event)
    {
        _domainEvents.Add(@event);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}

public enum ProductStatus
{
    InStock = 1,
    OutOfStock = 2,
}