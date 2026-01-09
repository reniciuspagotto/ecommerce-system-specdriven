using Application.DTOs;
using Domain.Aggregates;

namespace Application.Mappings;

public static class ProductMapper
{
    public static ProductDto ToDto(Product product)
    {
        return new ProductDto(
            product.Id,
            product.Name.Value,
            product.Price.Amount,
            product.Quantity.Value,
            product.CreatedAt,
            product.UpdatedAt,
            product.Version
        );
    }
}
