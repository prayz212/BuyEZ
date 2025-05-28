using Shared.Common;

namespace OrderAPI.Application.Domain;

public class OrderItem : AuditableEntity
{
    public string Id { get; init; } = string.Empty;

    public int Quantity { get; private set; }

    public double TotalPrice { get; private set; } = 0.0;

    public string OrderId { get; private set; } = string.Empty;

    // Copy of relevant properties
    public string ProductId { get; private set; } = string.Empty;

    public string ProductName { get; private set; } = string.Empty;

    public double ProductPrice { get; private set; }

    // Constructors
    /*
        - By default, compiler will create a default parameterless constructor.
            => Compiler will NOT generate a parameterless constructor in case we already defined parameters constructor
     
        - Parameterless constructor for EF Core to instantiate the object with the data got from database
     */
    internal OrderItem() { } 

    private OrderItem(
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

    public static OrderItem CreateNew(
        ProductReference product,
        int quantity,
        string createdBy)
    {
        return new(
            product.Id,
            product.Name,
            product.Price,
            quantity,
            createdBy);
    }

    private void UpdateTotalPrice()
    {
        TotalPrice = Math.Round(ProductPrice * Quantity, 2, MidpointRounding.AwayFromZero);
    }
}