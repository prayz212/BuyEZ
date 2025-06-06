namespace Shared.IntegrationEvents;

public class OrderCreatedIntegrationEvent : IntegrationEvent
{
    public string OrderId { get; init; } = string.Empty;

    public double TotalAmount { get; init; }

    public IDictionary<string, int> OrderItems { get; init; } = new Dictionary<string, int>();

    // TODO: refactor to ValueObject
    public string CardNumber { get; init; } = string.Empty;

    public string CardHolderName { get; init; } = string.Empty;

    public string ExpirationDate { get; init; } = string.Empty;

    public string SecurityCode { get; init; } = string.Empty;
}