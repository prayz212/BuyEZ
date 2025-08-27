using Shared.Common;

namespace CatalogAPI.Application.Domain.Events;

public class ProductDetailsUpdatedDomainEvent : DomainEvent
{
    public string ProductId { get; }

    public string ProductName { get; }

    public double ProductPrice { get; }

    public ProductDetailsUpdatedDomainEvent(string id, string name, double price)
    {
        ProductId = id;
        ProductName = name;
        ProductPrice = price;
    }
}