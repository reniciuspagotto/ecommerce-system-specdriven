using Domain.Aggregates;
using Domain.Exceptions;
using Domain.ValueObjects;
using FluentAssertions;
using Infrastructure.Repositories;
using Xunit;

namespace Infrastructure.Tests.Repositories;

public class InMemoryProductRepositoryTests
{
    [Fact]
    public async Task GetByIdAsync_ShouldReturnProduct_WhenProductExists()
    {
        // Arrange
        var repository = new InMemoryProductRepository();
        var product = Product.Create(
            new ProductName("Gaming Laptop"),
            new Money(1299.99m),
            new StockQuantity(50)
        );
        await repository.AddAsync(product);

        // Act
        var result = await repository.GetByIdAsync(product.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(product.Id);
        result.Name.Value.Should().Be("Gaming Laptop");
        result.Price.Amount.Should().Be(1299.99m);
        result.Quantity.Value.Should().Be(50);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenProductDoesNotExist()
    {
        // Arrange
        var repository = new InMemoryProductRepository();
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await repository.GetByIdAsync(nonExistentId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task AddAsync_ShouldStoreProduct()
    {
        // Arrange
        var repository = new InMemoryProductRepository();
        var product = Product.Create(
            new ProductName("Gaming Laptop"),
            new Money(1299.99m),
            new StockQuantity(50)
        );

        // Act
        await repository.AddAsync(product);
        var retrieved = await repository.GetByIdAsync(product.Id);

        // Assert
        retrieved.Should().NotBeNull();
        retrieved!.Id.Should().Be(product.Id);
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveProduct()
    {
        // Arrange
        var repository = new InMemoryProductRepository();
        var product = Product.Create(
            new ProductName("Gaming Laptop"),
            new Money(1299.99m),
            new StockQuantity(50)
        );
        await repository.AddAsync(product);

        // Act
        await repository.DeleteAsync(product.Id);
        var result = await repository.GetByIdAsync(product.Id);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateProduct_WhenVersionMatches()
    {
        // Arrange
        var repository = new InMemoryProductRepository();
        var product = Product.Create(
            new ProductName("Gaming Laptop"),
            new Money(1299.99m),
            new StockQuantity(50)
        );
        await repository.AddAsync(product);

        product.UpdateDetails(
            new ProductName("Gaming Laptop Pro"),
            new Money(1499.99m),
            new StockQuantity(30)
        );

        // Act
        await repository.UpdateAsync(product);
        var updated = await repository.GetByIdAsync(product.Id);

        // Assert
        updated.Should().NotBeNull();
        updated!.Name.Value.Should().Be("Gaming Laptop Pro");
        updated.Price.Amount.Should().Be(1499.99m);
        updated.Quantity.Value.Should().Be(30);
        updated.Version.Should().Be(2);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowProductNotFoundException_WhenProductDoesNotExist()
    {
        // Arrange
        var repository = new InMemoryProductRepository();
        var product = Product.Create(
            new ProductName("Gaming Laptop"),
            new Money(1299.99m),
            new StockQuantity(50)
        );

        // Act
        Func<Task> act = async () => await repository.UpdateAsync(product);

        // Assert
        await act.Should().ThrowAsync<ProductNotFoundException>()
            .WithMessage($"*{product.Id}*");
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowProductConcurrencyException_WhenVersionMismatch()
    {
        // Arrange
        var repository = new InMemoryProductRepository();
        var product = Product.Create(
            new ProductName("Gaming Laptop"),
            new Money(1299.99m),
            new StockQuantity(50)
        );
        await repository.AddAsync(product);

        // User A fetches the product (version 1)
        var userAProduct = await repository.GetByIdAsync(product.Id);
        
        // User B fetches the product (version 1)
        var userBProduct = await repository.GetByIdAsync(product.Id);

        // User A updates successfully (version 1 -> 2)
        userAProduct!.UpdateDetails(
            new ProductName("Gaming Laptop Pro"),
            new Money(1499.99m),
            new StockQuantity(30)
        );
        await repository.UpdateAsync(userAProduct);

        // User B tries to update with stale version (still has version 2 after UpdateDetails, but repo has version 2)
        // We need to simulate User B holding version 1 snapshot
        userBProduct!.UpdateDetails(
            new ProductName("Gaming Laptop Ultra"),
            new Money(1699.99m),
            new StockQuantity(20)
        );

        // Act - User B's update should fail because repo version (2) is ahead
        // Since both mutations happen on same instance due to in-memory references,
        // we'll manually simulate stale version by creating a fresh product with old version
        // For proper concurrency test with real persistence, this would be two separate instances
        
        // Fetch again to get version 2
        var currentProduct = await repository.GetByIdAsync(product.Id);
        currentProduct!.UpdateDetails(
            new ProductName("Another Update"),
            new Money(1599.99m),
            new StockQuantity(25)
        );
        await repository.UpdateAsync(currentProduct); // Now version 3

        // Simulate stale update attempt with version 2 (userBProduct is now version 2)
        Func<Task> act = async () => await repository.UpdateAsync(userBProduct);

        // Assert
        await act.Should().ThrowAsync<ProductConcurrencyException>()
            .WithMessage("*version*");
    }
}
