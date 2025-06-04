namespace Shared.IntegrationEvents;

public class OrderPlacedIntegrationEvent : IntegrationEvent
{
    public string OrderId { get; init; } = string.Empty;
}