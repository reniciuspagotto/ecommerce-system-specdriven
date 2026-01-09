using FluentAssertions;
using Xunit;

namespace Domain.Tests.ValueObjects;

public class ProductNameTests
{
    [Fact]
    public void ProductName_ShouldCreateWithValidValue()
    {
        // Arrange
        var validName = "Gaming Laptop";

        // Act
        var productName = new ProductName(validName);

        // Assert
        productName.Value.Should().Be(validName);
    }

    [Fact]
    public void ProductName_ShouldThrowException_WhenValueIsNull()
    {
        // Arrange
        string? nullName = null;

        // Act
        Action act = () => new ProductName(nullName!);

        // Assert
        act.Should().Throw<ProductValidationException>()
            .WithMessage("*name*null*");
    }

    [Fact]
    public void ProductName_ShouldThrowException_WhenValueIsEmpty()
    {
        // Arrange
        var emptyName = string.Empty;

        // Act
        Action act = () => new ProductName(emptyName);

        // Assert
        act.Should().Throw<ProductValidationException>()
            .WithMessage("*name*empty*");
    }

    [Fact]
    public void ProductName_ShouldThrowException_WhenValueIsWhitespaceOnly()
    {
        // Arrange
        var whitespaceName = "   ";

        // Act
        Action act = () => new ProductName(whitespaceName);

        // Assert
        act.Should().Throw<ProductValidationException>()
            .WithMessage("*whitespace*");
    }

    [Fact]
    public void ProductName_ShouldThrowException_WhenValueExceedsMaxLength()
    {
        // Arrange
        var longName = new string('a', 201);

        // Act
        Action act = () => new ProductName(longName);

        // Assert
        act.Should().Throw<ProductValidationException>()
            .WithMessage("*200 characters*");
    }

    [Fact]
    public void ProductName_ShouldAcceptMaxLength()
    {
        // Arrange
        var maxLengthName = new string('a', 200);

        // Act
        var productName = new ProductName(maxLengthName);

        // Assert
        productName.Value.Should().HaveLength(200);
    }

    [Fact]
    public void ProductName_ShouldAcceptUnicodeCharacters()
    {
        // Arrange
        var unicodeName = "Café Münchën 日本語";

        // Act
        var productName = new ProductName(unicodeName);

        // Assert
        productName.Value.Should().Be(unicodeName);
    }

    [Fact]
    public void ProductName_ShouldRejectControlCharacters()
    {
        // Arrange
        var nameWithControlChar = "Product\u0000Name";

        // Act
        Action act = () => new ProductName(nameWithControlChar);

        // Assert
        act.Should().Throw<ProductValidationException>()
            .WithMessage("*control characters*");
    }

    [Fact]
    public void ProductName_ShouldBeEqual_WhenValuesMatch()
    {
        // Arrange
        var name1 = new ProductName("Gaming Laptop");
        var name2 = new ProductName("Gaming Laptop");

        // Act & Assert
        name1.Should().Be(name2);
        (name1 == name2).Should().BeTrue();
    }

    [Fact]
    public void ProductName_ShouldNotBeEqual_WhenValuesDiffer()
    {
        // Arrange
        var name1 = new ProductName("Gaming Laptop");
        var name2 = new ProductName("Office Laptop");

        // Act & Assert
        name1.Should().NotBe(name2);
        (name1 != name2).Should().BeTrue();
    }
}
