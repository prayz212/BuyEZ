using ClientManagementAPI.Application.Domain.Common;
using ClientManagementAPI.Application.Domain.Dtos;

using Shared.Common;
using Shared.Common.Enums;
using Shared.Common.Exceptions;
using Shared.Common.Interfaces;

namespace ClientManagementAPI.Application.Domain;

public class Client : AuditableEntity, IAggregateRoot
{
    public string Id { get; init; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public string AliasName { get; private set; } = string.Empty;

    public string BriefDescription { get; private set; } = string.Empty;

    public SubscriptionType SubscriptionType { get; private set; }

    public ProductType[] RegisteredProductType { get; private set; } = [];

    public DateTimeOffset ValidUntil { get; private set; } = DateTimeOffset.UtcNow.AddYears(1);

    public bool IsActivated { get; private set; } = false;

    // Navigation property for the related Client
    public Image? Logo { get; private set; }

    // Domain Events property
    private readonly List<DomainEvent> _domainEvents = [];
    public IReadOnlyList<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    // Constructors
    /*
        - By default, compiler will create a default parameterless constructor.
            => Compiler will NOT generate a parameterless constructor in case we already defined parameters constructor
     
        - Parameterless constructor for EF Core to instantiate the object with the data got from database
     */
    internal Client() { }

    private Client(
        string name,
        string aliasName,
        string briefDescription,
        SubscriptionType subscription,
        ProductType[] productTypes,
        DateTimeOffset validUntil,
        Image? logo,
        string createdBy)
    {
        Id = Guid.NewGuid().ToString();
        Name = name;
        AliasName = aliasName;
        BriefDescription = briefDescription;
        SubscriptionType = subscription;
        RegisteredProductType = productTypes;
        ValidUntil = validUntil;
        IsActivated = false;
        CreatedBy = createdBy;

        Logo = logo;

        // TODO: Add Client created domain event
    }

    public static Client CreateNew(
        string name,
        string aliasName,
        string briefDescription,
        SubscriptionType subscription,
        ProductType[] productTypes,
        ClientImagePayload? clientImage,
        string createdBy)
    {
        if (!productTypes.Any())
            throw new ValidationException("At least one product type must be selected.");

        if (productTypes.Distinct().Count() != productTypes.Length)
            throw new ValidationException("Contains duplicate product types.");

        if (ExceededAllowedProductTypes(subscription, productTypes))
            throw new ValidationException("Exceeded the maximum allowed product types for current subscription.");

        var validUntilDateTime = GetValidUntilDateTimeBySubscriptionType(subscription);

        Image? logo = default;
        if (clientImage != null)
            logo = Image.CreateNew(clientImage, createdBy);

        return new(name, aliasName, briefDescription, subscription, productTypes, validUntilDateTime, logo, createdBy);
    }

    private static DateTimeOffset GetValidUntilDateTimeBySubscriptionType(SubscriptionType subscription)
        => subscription switch
        {
            SubscriptionType.Basic => DateTimeOffset.UtcNow.AddYears(1),
            SubscriptionType.Standard => DateTimeOffset.UtcNow.AddYears(2),
            SubscriptionType.Premium => DateTimeOffset.UtcNow.AddYears(5),
            _ => throw new InvalidOperationException("Invalid SubscriptionType"),
        };

    private static bool ExceededAllowedProductTypes(SubscriptionType subscription, ProductType[] productTypes)
        => subscription switch
        {
            SubscriptionType.Basic =>
                productTypes.Length > ClientConstants.MAXIMUM_PRODUCT_TYPES_BASIC_SUB,
            SubscriptionType.Standard =>
                productTypes.Length > ClientConstants.MAXIMUM_PRODUCT_TYPES_STANDARD_SUB,
            SubscriptionType.Premium => false,
            _ => throw new InvalidOperationException("Invalid SubscriptionType")
        };

    public void Activate(string modifiedBy)
    {
        if (IsActivated)
            throw new ValidationException("Client already activated.");

        IsActivated = true;
        LastModifiedBy = modifiedBy;
    }

    public void Deactivate(string modifiedBy)
    {
        if (!IsActivated)
            throw new ValidationException("Client not yet activated.");

        IsActivated = false;
        LastModifiedBy = modifiedBy;
    }

    public void UpdateDetails(
        string name,
        string briefDescription,
        SubscriptionType subscription,
        ProductType[] productTypes,
        ClientImagePayload? clientImage,
        string modifiedBy)
    {
        if (!IsActivated)
            throw new ValidationException($"Client is not activated.");

        if (!productTypes.Any())
            throw new ValidationException("At least one product type must be selected.");

        if (productTypes.Distinct().Count() != productTypes.Length)
            throw new ValidationException("Contains duplicate product types.");

        if (ExceededAllowedProductTypes(subscription, productTypes))
            throw new ValidationException("Exceeded the maximum allowed product types for current subscription.");

        var validUntilDateTime = GetValidUntilDateTimeBySubscriptionType(subscription);

        Name = name;
        BriefDescription = briefDescription;
        SubscriptionType = subscription;
        RegisteredProductType = productTypes;
        ValidUntil = validUntilDateTime;
        LastModifiedBy = modifiedBy;

        /* Once Logo is set, just allow to changes */
        if (clientImage != null
            && clientImage.Filename != Logo?.Filename
            && clientImage.URL != Logo?.URL)
            Logo = Image.CreateNew(clientImage, modifiedBy);
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

public enum SubscriptionType
{
    /*
        Having only one account (master) to do all the management things. 
        Limited 3 registered product types.
        1-year subscription
    */
    Basic = 1,

    /*
        Having 3 accounts (1 master, 2 supporters) to do all the management things.
        Limited 10 registered product types.
        2-year subscription
    */
    Standard,

    /*
        Having 10 accounts (1 master, 2 administrators, 7 supporters) to do all the management things.
        Unlimited registered product types.
        5-year subscription
    */
    Premium
}