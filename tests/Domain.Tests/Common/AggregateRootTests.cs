using FluentAssertions;
using Xunit;

namespace Domain.Tests.Common;

public class AggregateRootTests
{
    private class TestAggregate : AggregateRoot
    {
        public TestAggregate(Guid id, DateTime createdAt, DateTime updatedAt, int version)
        {
            Id = id;
            CreatedAt = createdAt;
            UpdatedAt = updatedAt;
            Version = version;
        }
    }

    [Fact]
    public void AggregateRoot_ShouldHaveId()
    {
        // Arrange
        var id = Guid.NewGuid();
        var now = DateTime.UtcNow;

        // Act
        var aggregate = new TestAggregate(id, now, now, 1);

        // Assert
        aggregate.Id.Should().Be(id);
    }

    [Fact]
    public void AggregateRoot_ShouldHaveCreatedAt()
    {
        // Arrange
        var id = Guid.NewGuid();
        var now = DateTime.UtcNow;

        // Act
        var aggregate = new TestAggregate(id, now, now, 1);

        // Assert
        aggregate.CreatedAt.Should().Be(now);
    }

    [Fact]
    public void AggregateRoot_ShouldHaveUpdatedAt()
    {
        // Arrange
        var id = Guid.NewGuid();
        var now = DateTime.UtcNow;

        // Act
        var aggregate = new TestAggregate(id, now, now, 1);

        // Assert
        aggregate.UpdatedAt.Should().Be(now);
    }

    [Fact]
    public void AggregateRoot_ShouldHaveVersion()
    {
        // Arrange
        var id = Guid.NewGuid();
        var now = DateTime.UtcNow;

        // Act
        var aggregate = new TestAggregate(id, now, now, 1);

        // Assert
        aggregate.Version.Should().Be(1);
    }

    [Fact]
    public void AggregateRoot_VersionShouldIncrement()
    {
        // Arrange
        var id = Guid.NewGuid();
        var now = DateTime.UtcNow;
        var aggregate = new TestAggregate(id, now, now, 1);

        // Act
        var updatedAggregate = new TestAggregate(id, now, DateTime.UtcNow, 2);

        // Assert
        updatedAggregate.Version.Should().Be(2);
    }
}
