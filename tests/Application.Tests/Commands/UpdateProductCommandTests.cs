using Application.Commands;
using FluentAssertions;
using Xunit;

namespace Application.Tests.Commands;

public class UpdateProductCommandTests
{
    [Fact]
    public void UpdateProductCommand_WithValidData_ShouldCreate()
    {
        // Arrange & Act
        var command = new UpdateProductCommand(
            Guid.NewGuid(),
            "Updated Product",
            99.99m,
            10,
            1
        );

        // Assert
        command.Id.Should().NotBeEmpty();
        command.Name.Should().Be("Updated Product");
        command.Price.Should().Be(99.99m);
        command.Quantity.Should().Be(10);
        command.Version.Should().Be(1);
    }

    [Fact]
    public void UpdateProductCommand_WithDifferentVersions_ShouldAllowAll()
    {
        // Arrange & Act
        var v1 = new UpdateProductCommand(Guid.NewGuid(), "Product", 10m, 5, 1);
        var v2 = new UpdateProductCommand(Guid.NewGuid(), "Product", 10m, 5, 2);
        var v10 = new UpdateProductCommand(Guid.NewGuid(), "Product", 10m, 5, 10);

        // Assert
        v1.Version.Should().Be(1);
        v2.Version.Should().Be(2);
        v10.Version.Should().Be(10);
    }
}
