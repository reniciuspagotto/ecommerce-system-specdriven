using FluentAssertions;
using Xunit;

namespace Domain.Tests.ValueObjects;

public class MoneyTests
{
    [Fact]
    public void Money_ShouldCreateWithValidAmount()
    {
        // Arrange
        var validAmount = 99.99m;

        // Act
        var money = new Money(validAmount);

        // Assert
        money.Amount.Should().Be(validAmount);
    }

    [Fact]
    public void Money_ShouldThrowException_WhenAmountIsZero()
    {
        // Arrange
        var zeroAmount = 0m;

        // Act
        Action act = () => new Money(zeroAmount);

        // Assert
        act.Should().Throw<ProductValidationException>()
            .WithMessage("*greater than zero*");
    }

    [Fact]
    public void Money_ShouldThrowException_WhenAmountIsNegative()
    {
        // Arrange
        var negativeAmount = -10m;

        // Act
        Action act = () => new Money(negativeAmount);

        // Assert
        act.Should().Throw<ProductValidationException>()
            .WithMessage("*greater than zero*");
    }

    [Fact]
    public void Money_ShouldAcceptSmallPositiveAmount()
    {
        // Arrange
        var smallAmount = 0.01m;

        // Act
        var money = new Money(smallAmount);

        // Assert
        money.Amount.Should().Be(smallAmount);
    }

    [Fact]
    public void Money_ShouldAcceptLargeAmount()
    {
        // Arrange
        var largeAmount = 999999.99m;

        // Act
        var money = new Money(largeAmount);

        // Assert
        money.Amount.Should().Be(largeAmount);
    }

    [Fact]
    public void Money_ShouldBeEqual_WhenAmountsMatch()
    {
        // Arrange
        var money1 = new Money(99.99m);
        var money2 = new Money(99.99m);

        // Act & Assert
        money1.Should().Be(money2);
        (money1 == money2).Should().BeTrue();
    }

    [Fact]
    public void Money_ShouldNotBeEqual_WhenAmountsDiffer()
    {
        // Arrange
        var money1 = new Money(99.99m);
        var money2 = new Money(100.00m);

        // Act & Assert
        money1.Should().NotBe(money2);
        (money1 != money2).Should().BeTrue();
    }
}
