using Domain.Exceptions;

namespace Domain.ValueObjects;

public record Money
{
    public decimal Amount { get; }

    public Money(decimal amount)
    {
        if (amount <= 0)
            throw new ProductValidationException("Price must be greater than zero.");

        Amount = amount;
    }
}
