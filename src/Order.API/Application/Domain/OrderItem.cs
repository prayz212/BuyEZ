using OrderAPI.Application.Shared.Dtos;
using Shared.Common;

namespace OrderAPI.Application.Domain;

public class OrderItem : AuditableEntity
{
    public string Id { get; private set; } = string.Empty;

    public int Quantity { get; private set; }

    public double TotalPrice { get; private set; } = 0.0;

    public string OrderId { get; set; } = string.Empty;

    // Copy of relevant properties
    public string ProductId { get; private set; } = string.Empty;

    public string ProductName { get; private set; } = string.Empty;

    public double ProductPrice { get; private set; }

    // Navigation Properties
    public Order? Order { get; set; }

    // Constructors
    /*
        - By default, compiler will create a default parameterless constructor.
            => Compiler will NOT generate a parameterless constructor in case we already defined parameters constructor
     
        - Parameterless constructor for EF Core to instantiate the object with the data got from database
     */
    internal OrderItem() { } 

    public OrderItem(
        string productId, string productName, double productPrice, int quantity, string createdBy)
    {
        Id = Guid.NewGuid().ToString();
        ProductId = productId;
        ProductName = productName;
        ProductPrice = productPrice;
        Quantity = quantity;
        CreatedBy = createdBy;
        
        UpdateTotalPrice();
    }

    private void UpdateTotalPrice() 
    {
        TotalPrice = Math.Round(ProductPrice * Quantity, 2, MidpointRounding.AwayFromZero);
    }        

    public static OrderItemResponse ToDto(OrderItem orderItem)
    {
        return new OrderItemResponse(
            orderItem.ProductId,
            orderItem.ProductName,
            orderItem.ProductPrice,
            orderItem.Quantity,
            orderItem.TotalPrice
        );
    }
}