using Application.Commands;
using Application.Services;
using Domain.Aggregates;
using Domain.Repositories;
using FluentAssertions;
using Xunit;

namespace Application.Tests.Commands;

public class CreateProductCommandTests
{
    private class MockProductRepository : IProductRepository
    {
        private readonly List<Product> _products = new();

        public Task<Product> AddAsync(Product product)
        {
            _products.Add(product);
            return Task.FromResult(product);
        }

        public Task UpdateAsync(Product product)
        {
            throw new NotImplementedException();
        }

        public Task<Product?> GetByIdAsync(Guid id)
        {
            return Task.FromResult(_products.FirstOrDefault(p => p.Id == id));
        }

        public Task DeleteAsync(Guid id)
        {
            throw new NotImplementedException();
        }
    }

    [Fact]
    public async Task CreateProductCommand_ShouldCreateProduct_WithValidData()
    {
        // Arrange
        var repository = new MockProductRepository();
        var service = new ProductApplicationService(repository);
        var command = new CreateProductCommand("Gaming Laptop", 1299.99m, 50);

        // Act
        var result = await service.CreateAsync(command);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();
        result.Name.Should().Be("Gaming Laptop");
        result.Price.Should().Be(1299.99m);
        result.Quantity.Should().Be(50);
        result.Version.Should().Be(1);
    }

    [Fact]
    public async Task CreateProductCommand_ShouldThrowException_WhenNameIsEmpty()
    {
        // Arrange
        var repository = new MockProductRepository();
        var service = new ProductApplicationService(repository);
        var command = new CreateProductCommand("", 1299.99m, 50);

        // Act
        Func<Task> act = async () => await service.CreateAsync(command);

        // Assert
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("*name*empty*");
    }

    [Fact]
    public async Task CreateProductCommand_ShouldThrowException_WhenPriceIsZero()
    {
        // Arrange
        var repository = new MockProductRepository();
        var service = new ProductApplicationService(repository);
        var command = new CreateProductCommand("Gaming Laptop", 0, 50);

        // Act
        Func<Task> act = async () => await service.CreateAsync(command);

        // Assert
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("*greater than zero*");
    }

    [Fact]
    public async Task CreateProductCommand_ShouldThrowException_WhenQuantityIsNegative()
    {
        // Arrange
        var repository = new MockProductRepository();
        var service = new ProductApplicationService(repository);
        var command = new CreateProductCommand("Gaming Laptop", 1299.99m, -1);

        // Act
        Func<Task> act = async () => await service.CreateAsync(command);

        // Assert
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("*cannot be negative*");
    }

    [Fact]
    public async Task CreateProductCommand_ShouldAllowZeroQuantity()
    {
        // Arrange
        var repository = new MockProductRepository();
        var service = new ProductApplicationService(repository);
        var command = new CreateProductCommand("Gaming Laptop", 1299.99m, 0);

        // Act
        var result = await service.CreateAsync(command);

        // Assert
        result.Quantity.Should().Be(0);
    }
}
