using Domain.Exceptions;

namespace Domain.ValueObjects;

public record StockQuantity
{
    public int Value { get; }

    public StockQuantity(int value)
    {
        if (value < 0)
            throw new ProductValidationException("Stock quantity cannot be negative.");

        Value = value;
    }
}
