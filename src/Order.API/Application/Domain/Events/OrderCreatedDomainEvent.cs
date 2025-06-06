using Shared.Common;

namespace OrderAPI.Application.Domain.Events;

public class OrderCreatedDomainEvent : DomainEvent
{
    public string OrderId { get; }

    public double TotalAmount { get; }

    public IReadOnlyList<OrderItem> OrderItems { get; }

    // TODO: refactor to ValueObject
    public string CardNumber { get; }

    public string CardHolderName { get; }

    public string ExpirationDate { get; }

    public string SecurityCode { get; }

    public OrderCreatedDomainEvent(
        string id,
        double amount,
        IReadOnlyList<OrderItem> items,
        string cardNumber,
        string cardHolderName,
        string expirationDate,
        string securityCode)
    {
        OrderId = id;
        TotalAmount = amount;
        OrderItems = items;
        CardNumber = cardNumber;
        CardHolderName = cardHolderName;
        SecurityCode = securityCode;
        ExpirationDate = expirationDate;
    }
}