namespace OrderAPI.Application.Domain;

public class ProductReference
{
    public string Id { get; init; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public double Price { get; private set; }
}