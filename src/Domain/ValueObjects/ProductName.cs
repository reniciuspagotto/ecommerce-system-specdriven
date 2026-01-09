using Domain.Exceptions;

namespace Domain.ValueObjects;

public record ProductName
{
    public string Value { get; }

    public ProductName(string value)
    {
        if (value == null)
            throw new ProductValidationException("Product name cannot be null.");

        if (string.IsNullOrEmpty(value))
            throw new ProductValidationException("Product name cannot be empty.");

        if (string.IsNullOrWhiteSpace(value))
            throw new ProductValidationException("Product name cannot be whitespace only.");

        if (value.Length > 200)
            throw new ProductValidationException("Product name cannot exceed 200 characters.");

        if (ContainsControlCharacters(value))
            throw new ProductValidationException("Product name cannot contain control characters.");

        Value = value;
    }

    private static bool ContainsControlCharacters(string value)
    {
        foreach (char c in value)
        {
            if (char.IsControl(c))
                return true;
        }
        return false;
    }
}
