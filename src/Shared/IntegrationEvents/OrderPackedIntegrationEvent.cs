namespace Shared.IntegrationEvents;

public class OrderPackedIntegrationEvent : IntegrationEvent
{
    public string OrderId { get; init; } = string.Empty;
}