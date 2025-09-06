namespace Shared.IntegrationEvents;

public class OrderCreatedIntegrationEvent : IntegrationEvent
{
    public string OrderId { get; init; } = string.Empty;

    public double TotalAmount { get; init; }

    public IDictionary<string, int> OrderItems { get; init; } = new Dictionary<string, int>();
}