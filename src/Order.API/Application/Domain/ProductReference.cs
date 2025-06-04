namespace OrderAPI.Application.Domain;

public class ProductReference
{
    public string Id { get; init; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public double Price { get; private set; }

    // Constructors
    /*
        - By default, compiler will create a default parameterless constructor.
            => Compiler will NOT generate a parameterless constructor in case we already defined parameters constructor
     
        - Parameterless constructor for EF Core to instantiate the object with the data got from database
     */
    internal ProductReference() { }

    private ProductReference(string id, string name, double price)
    {
        Id = id;
        Name = name;
        Price = price;
    }

    public static ProductReference CreateNew(string id, string name, double price)
    {
        return new(id, name, price);
    }
}