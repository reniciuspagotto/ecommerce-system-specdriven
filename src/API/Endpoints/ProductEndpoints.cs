using API.Requests;
using Application.Commands;
using Application.Services;

namespace API.Endpoints;

public static class ProductEndpoints
{
    public static void MapProductEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/products");

        group.MapPost("", async (CreateProductRequest request, ProductApplicationService service) =>
        {
            var command = new CreateProductCommand(request.Name, request.Price, request.Quantity);
            var result = await service.CreateAsync(command);
            
            return Results.Created($"/api/products/{result.Id}", result);
        });

        group.MapGet("{id:guid}", async (Guid id, ProductApplicationService service) =>
        {
            var result = await service.GetByIdAsync(id);
            return Results.Ok(result);
        });

        group.MapPut("{id:guid}", async (Guid id, UpdateProductRequest request, ProductApplicationService service) =>
        {
            var command = new UpdateProductCommand(id, request.Name, request.Price, request.Quantity, request.Version);
            var result = await service.UpdateAsync(command);
            
            return Results.Ok(result);
        });

        group.MapDelete("{id:guid}", async (Guid id, ProductApplicationService service) =>
        {
            await service.DeleteAsync(id);
            return Results.NoContent();
        });
    }
}
