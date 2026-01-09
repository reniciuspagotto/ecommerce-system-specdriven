using Domain.Common;
using Domain.Exceptions;
using Domain.ValueObjects;

namespace Domain.Aggregates;

public class Product : AggregateRoot
{
    public ProductName Name { get; private set; }
    public Money Price { get; private set; }
    public StockQuantity Quantity { get; private set; }

    private Product(ProductName name, Money price, StockQuantity quantity)
    {
        Name = name;
        Price = price;
        Quantity = quantity;
    }

    public static Product Create(ProductName name, Money price, StockQuantity quantity)
    {
        var product = new Product(name, price, quantity)
        {
            Id = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Version = 1
        };

        return product;
    }

    public void UpdateDetails(ProductName name, Money price, StockQuantity quantity)
    {
        Name = name;
        Price = price;
        Quantity = quantity;
        Version++;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Creates a shallow clone of the product for concurrency testing in in-memory scenarios.
    /// </summary>
    public Product Clone()
    {
        var clone = new Product(Name, Price, Quantity)
        {
            Id = this.Id,
            CreatedAt = this.CreatedAt,
            UpdatedAt = this.UpdatedAt,
            Version = this.Version
        };
        return clone;
    }
}
