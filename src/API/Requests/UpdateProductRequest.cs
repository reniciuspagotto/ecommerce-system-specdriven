namespace API.Requests;

public record UpdateProductRequest(string Name, decimal Price, int Quantity, int Version);
