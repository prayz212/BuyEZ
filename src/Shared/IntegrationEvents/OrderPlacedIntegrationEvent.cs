namespace Shared.IntegrationEvents;

public class OrderPlacedIntegrationEvent : IntegrationEvent
{
    public string OrderId { get; }

    public OrderPlacedIntegrationEvent(string id)
    {
        OrderId = id;
    }
}