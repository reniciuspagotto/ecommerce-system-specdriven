using FluentAssertions;
using Xunit;

namespace Domain.Tests.ValueObjects;

public class StockQuantityTests
{
    [Fact]
    public void StockQuantity_ShouldCreateWithValidValue()
    {
        // Arrange
        var validQuantity = 100;

        // Act
        var stockQuantity = new StockQuantity(validQuantity);

        // Assert
        stockQuantity.Value.Should().Be(validQuantity);
    }

    [Fact]
    public void StockQuantity_ShouldAcceptZero()
    {
        // Arrange
        var zeroQuantity = 0;

        // Act
        var stockQuantity = new StockQuantity(zeroQuantity);

        // Assert
        stockQuantity.Value.Should().Be(0);
    }

    [Fact]
    public void StockQuantity_ShouldThrowException_WhenValueIsNegative()
    {
        // Arrange
        var negativeQuantity = -1;

        // Act
        Action act = () => new StockQuantity(negativeQuantity);

        // Assert
        act.Should().Throw<ProductValidationException>()
            .WithMessage("*cannot be negative*");
    }

    [Fact]
    public void StockQuantity_ShouldAcceptLargeValue()
    {
        // Arrange
        var largeQuantity = 999999;

        // Act
        var stockQuantity = new StockQuantity(largeQuantity);

        // Assert
        stockQuantity.Value.Should().Be(largeQuantity);
    }

    [Fact]
    public void StockQuantity_ShouldBeEqual_WhenValuesMatch()
    {
        // Arrange
        var quantity1 = new StockQuantity(100);
        var quantity2 = new StockQuantity(100);

        // Act & Assert
        quantity1.Should().Be(quantity2);
        (quantity1 == quantity2).Should().BeTrue();
    }

    [Fact]
    public void StockQuantity_ShouldNotBeEqual_WhenValuesDiffer()
    {
        // Arrange
        var quantity1 = new StockQuantity(100);
        var quantity2 = new StockQuantity(200);

        // Act & Assert
        quantity1.Should().NotBe(quantity2);
        (quantity1 != quantity2).Should().BeTrue();
    }
}
