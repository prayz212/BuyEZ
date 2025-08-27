namespace Shared.IntegrationEvents;

public class DeliverySucceededIntegrationEvent : IntegrationEvent
{
    public string OrderId { get; init; } = string.Empty;
}