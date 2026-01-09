namespace Application.DTOs;

public record ProductDto(
    Guid Id,
    string Name,
    decimal Price,
    int Quantity,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    int Version
);
