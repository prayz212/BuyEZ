using Shared.Common;

namespace CatalogAPI.Application.Domain.Events;

public class RestockThresholdReachedDomainEvent : DomainEvent
{
    public string ProductId { get; }
    
    public int RemainingStockQuantity { get; }

    public RestockThresholdReachedDomainEvent(string productId, int remainingStockQuantity)
    {
        ProductId = productId;
        RemainingStockQuantity = remainingStockQuantity;
    }
}