namespace Shared.IntegrationEvents;

public class ProductCreatedIntegrationEvent : IntegrationEvent
{
    public string ProductId { get; init; } = string.Empty;

    public string ProductName { get; init; } = string.Empty;

    public double ProductPrice { get; init; }

    public ProductCreatedIntegrationEvent() { }

    public ProductCreatedIntegrationEvent(string id, string name, double price)
    {
        ProductId = id;
        ProductName = name;
        ProductPrice = price;
    }
}