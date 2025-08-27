namespace Shared.IntegrationEvents;

public class ProductDetailsUpdatedIntegrationEvent : IntegrationEvent
{
    public string ProductId { get; }

    public string ProductName { get; }

    public double ProductPrice { get; }

    public ProductDetailsUpdatedIntegrationEvent(string id, string name, double price)
    {
        ProductId = id;
        ProductName = name;
        ProductPrice = price;
    }
}