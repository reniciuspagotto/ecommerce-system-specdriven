namespace API.Requests;

public record CreateProductRequest(
    string Name,
    decimal Price,
    int Quantity
);
