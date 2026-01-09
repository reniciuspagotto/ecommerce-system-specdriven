using Domain.Aggregates;

namespace Domain.Repositories;

public interface IProductRepository
{
    Task<Product> AddAsync(Product product);
    Task UpdateAsync(Product product);
    Task<Product?> GetByIdAsync(Guid id);
    Task DeleteAsync(Guid id);
}
