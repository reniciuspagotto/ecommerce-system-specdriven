using Application.Commands;
using Application.DTOs;
using Application.Mappings;
using Domain.Aggregates;
using Domain.Exceptions;
using Domain.Repositories;
using Domain.ValueObjects;

namespace Application.Services;

public class ProductApplicationService
{
    private readonly IProductRepository _repository;

    public ProductApplicationService(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<ProductDto> CreateAsync(CreateProductCommand command)
    {
        var name = new ProductName(command.Name);
        var price = new Money(command.Price);
        var quantity = new StockQuantity(command.Quantity);

        var product = Product.Create(name, price, quantity);
        
        await _repository.AddAsync(product);

        return ProductMapper.ToDto(product);
    }

    public async Task<ProductDto> GetByIdAsync(Guid id)
    {
        var product = await _repository.GetByIdAsync(id);
        
        if (product == null)
        {
            throw new ProductNotFoundException(id);
        }

        return ProductMapper.ToDto(product);
    }

    public async Task<ProductDto> UpdateAsync(UpdateProductCommand command)
    {
        var product = await _repository.GetByIdAsync(command.Id);
        
        if (product == null)
        {
            throw new ProductNotFoundException(command.Id);
        }

        // Validate version matches before updating
        if (product.Version != command.Version)
        {
            throw new ProductConcurrencyException(
                command.Id,
                command.Version,
                product.Version
            );
        }

        var name = new ProductName(command.Name);
        var price = new Money(command.Price);
        var quantity = new StockQuantity(command.Quantity);

        product.UpdateDetails(name, price, quantity);
        
        await _repository.UpdateAsync(product);

        return ProductMapper.ToDto(product);
    }

    public async Task DeleteAsync(Guid id)
    {
        await _repository.DeleteAsync(id);
    }
}
