namespace Shared.IntegrationEvents;

public class OrderPackingStartedIntegrationEvent : IntegrationEvent
{
    public string OrderId { get; init; } = string.Empty;

    public string JobId { get; init; } = string.Empty;
}