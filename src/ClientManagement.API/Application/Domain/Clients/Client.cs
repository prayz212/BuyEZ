using ClientManagementAPI.Application.Common;
using ClientManagementAPI.Application.Features.Clients.Shared.Dtos;

namespace ClientManagementAPI.Application.Domain.Clients;

public class Client : AuditableEntity, IHasDomainEvent
{
    public string Id { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string AliasName { get; set; } = string.Empty;

    public string BriefDescription { get; set; } = string.Empty;

    public SubscriptionType SubscriptionType { get; set; }

    public ProductType[] RegisteredProductType { get; set;} = [];

    public DateTimeOffset ValidUntil { get; set; } = DateTimeOffset.Now.AddYears(1);

    public bool IsActivated { get; set; } = false;

    public List<DomainEvent> DomainEvents { get; } = [];

    // Navigation property for the related Client
    public Image? Logo { get; set; }

    public static ClientDetailResponse ToDto(Client client) => new 
    (
        client.Id,
        client.Name,
        client.AliasName,
        client.BriefDescription,
        client.SubscriptionType,
        client.RegisteredProductType,
        client.ValidUntil,
        client.IsActivated,
        client.Logo != null ? Image.ToDto(client.Logo) : null
    );
}

public enum SubscriptionType
{
    /*
        Having only one account (master) to do all the management things. 
        Limited 3 registered product types.
    */
    Basic = 1,

    /*
        Having 3 accounts (1 master, 2 supporters) to do all the management things.
        Limited 10 registered product types.
    */
    Standard,

    /*
        Having 10 accounts (1 master, 2 administrators, 7 supporters) to do all the management things.
        Unlimited registered product types.
    */
    Premium
}

// TODO: Refactor to centralize in share project
public enum ProductType
{
    /* CLOTHES */
    // Men
    Men_TShirt = 1,
    Men_Pants,
    Men_Jacket,

    // Women
    Women_Pants,
    Women_TShirt,
    Women_Jacket,
}