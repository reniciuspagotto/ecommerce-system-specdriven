using Domain.Aggregates;
using Domain.Exceptions;
using Domain.Repositories;
using System.Collections.Concurrent;

namespace Infrastructure.Repositories;

public class InMemoryProductRepository : IProductRepository
{
    private readonly ConcurrentDictionary<Guid, Product> _products = new();

    public Task<Product> AddAsync(Product product)
    {
        _products[product.Id] = product;
        return Task.FromResult(product);
    }

    public Task UpdateAsync(Product product)
    {
        if (!_products.TryGetValue(product.Id, out var existingProduct))
        {
            throw new ProductNotFoundException(product.Id);
        }

        // In-memory repository stores references; domain updates mutate the same instance.
        // Concurrency check: reject updates when the stored version is ahead of the incoming version.
        if (existingProduct.Version > product.Version)
        {
            throw new ProductConcurrencyException(
                product.Id,
                product.Version,
                existingProduct.Version
            );
        }

        _products[product.Id] = product;
        return Task.CompletedTask;
    }

    public Task<Product?> GetByIdAsync(Guid id)
    {
        // Return a clone to simulate repository retrieval behavior
        // and enable proper concurrency testing
        if (_products.TryGetValue(id, out var product))
        {
            return Task.FromResult<Product?>(product.Clone());
        }
        return Task.FromResult<Product?>(null);
    }

    public Task DeleteAsync(Guid id)
    {
        _products.TryRemove(id, out _);
        return Task.CompletedTask;
    }
}
