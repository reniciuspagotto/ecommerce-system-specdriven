namespace Application.Commands;

public record UpdateProductCommand(Guid Id, string Name, decimal Price, int Quantity, int Version);
