using Domain.Aggregates;
using Domain.Exceptions;
using Domain.ValueObjects;
using FluentAssertions;
using Xunit;

namespace Domain.Tests.Aggregates;

public class ProductTests
{
    [Fact]
    public void Product_Create_ShouldCreateValidProduct()
    {
        // Arrange
        var name = new ProductName("Gaming Laptop");
        var price = new Money(1299.99m);
        var quantity = new StockQuantity(50);

        // Act
        var product = Product.Create(name, price, quantity);

        // Assert
        product.Should().NotBeNull();
        product.Id.Should().NotBeEmpty();
        product.Name.Should().Be(name);
        product.Price.Should().Be(price);
        product.Quantity.Should().Be(quantity);
        product.Version.Should().Be(1);
        product.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        product.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        product.UpdatedAt.Should().BeCloseTo(product.CreatedAt, TimeSpan.FromMilliseconds(10));
    }

    [Fact]
    public void Product_Create_ShouldGenerateUniqueIds()
    {
        // Arrange
        var name = new ProductName("Gaming Laptop");
        var price = new Money(1299.99m);
        var quantity = new StockQuantity(50);

        // Act
        var product1 = Product.Create(name, price, quantity);
        var product2 = Product.Create(name, price, quantity);

        // Assert
        product1.Id.Should().NotBe(product2.Id);
    }

    [Fact]
    public void Product_Create_ShouldThrowException_WhenNameIsInvalid()
    {
        // Arrange
        var price = new Money(1299.99m);
        var quantity = new StockQuantity(50);

        // Act
        Action act = () => new ProductName(string.Empty);

        // Assert
        act.Should().Throw<ProductValidationException>();
    }

    [Fact]
    public void Product_Create_ShouldThrowException_WhenPriceIsInvalid()
    {
        // Arrange
        var name = new ProductName("Gaming Laptop");
        var quantity = new StockQuantity(50);

        // Act
        Action act = () => new Money(0);

        // Assert
        act.Should().Throw<ProductValidationException>();
    }

    [Fact]
    public void Product_Create_ShouldThrowException_WhenQuantityIsInvalid()
    {
        // Arrange
        var name = new ProductName("Gaming Laptop");
        var price = new Money(1299.99m);

        // Act
        Action act = () => new StockQuantity(-1);

        // Assert
        act.Should().Throw<ProductValidationException>();
    }

    [Fact]
    public void Product_UpdateDetails_ShouldUpdateProductAndIncrementVersion()
    {
        // Arrange
        var originalName = new ProductName("Gaming Laptop");
        var originalPrice = new Money(1299.99m);
        var originalQuantity = new StockQuantity(50);
        var product = Product.Create(originalName, originalPrice, originalQuantity);
        var originalCreatedAt = product.CreatedAt;
        
        // Wait a tiny bit to ensure UpdatedAt differs
        Thread.Sleep(10);

        var newName = new ProductName("Gaming Laptop Pro");
        var newPrice = new Money(1499.99m);
        var newQuantity = new StockQuantity(30);

        // Act
        product.UpdateDetails(newName, newPrice, newQuantity);

        // Assert
        product.Name.Should().Be(newName);
        product.Price.Should().Be(newPrice);
        product.Quantity.Should().Be(newQuantity);
        product.Version.Should().Be(2);
        product.CreatedAt.Should().Be(originalCreatedAt);
        product.UpdatedAt.Should().BeAfter(originalCreatedAt);
    }

    [Fact]
    public void Product_UpdateDetails_ShouldIncrementVersionMultipleTimes()
    {
        // Arrange
        var name = new ProductName("Gaming Laptop");
        var price = new Money(1299.99m);
        var quantity = new StockQuantity(50);
        var product = Product.Create(name, price, quantity);

        // Act
        product.UpdateDetails(name, price, new StockQuantity(40));
        product.UpdateDetails(name, price, new StockQuantity(30));
        product.UpdateDetails(name, price, new StockQuantity(20));

        // Assert
        product.Version.Should().Be(4);
        product.Quantity.Value.Should().Be(20);
    }
}
