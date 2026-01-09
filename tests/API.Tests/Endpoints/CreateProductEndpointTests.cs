using API.Requests;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace API.Tests.Endpoints;

public class CreateProductEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public CreateProductEndpointTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CreateProduct_ShouldReturn201_WithValidData()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new CreateProductRequest("Gaming Laptop", 1299.99m, 50);

        // Act
        var response = await client.PostAsJsonAsync("/api/products", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
        
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(content);
        
        result.GetProperty("id").GetGuid().Should().NotBeEmpty();
        result.GetProperty("name").GetString().Should().Be("Gaming Laptop");
        result.GetProperty("price").GetDecimal().Should().Be(1299.99m);
        result.GetProperty("quantity").GetInt32().Should().Be(50);
        result.GetProperty("version").GetInt32().Should().Be(1);
    }

    [Fact]
    public async Task CreateProduct_ShouldReturn400_WhenNameIsEmpty()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new CreateProductRequest("", 1299.99m, 50);

        // Act
        var response = await client.PostAsJsonAsync("/api/products", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        var content = await response.Content.ReadAsStringAsync();
        var problemDetails = JsonSerializer.Deserialize<JsonElement>(content);
        
        problemDetails.GetProperty("status").GetInt32().Should().Be(400);
        problemDetails.GetProperty("detail").GetString().Should().Contain("name");
    }

    [Fact]
    public async Task CreateProduct_ShouldReturn400_WhenPriceIsZero()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new CreateProductRequest("Gaming Laptop", 0, 50);

        // Act
        var response = await client.PostAsJsonAsync("/api/products", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        var content = await response.Content.ReadAsStringAsync();
        var problemDetails = JsonSerializer.Deserialize<JsonElement>(content);
        
        problemDetails.GetProperty("status").GetInt32().Should().Be(400);
        problemDetails.GetProperty("detail").GetString().Should().Contain("greater than zero");
    }

    [Fact]
    public async Task CreateProduct_ShouldReturn400_WhenQuantityIsNegative()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new CreateProductRequest("Gaming Laptop", 1299.99m, -1);

        // Act
        var response = await client.PostAsJsonAsync("/api/products", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        var content = await response.Content.ReadAsStringAsync();
        var problemDetails = JsonSerializer.Deserialize<JsonElement>(content);
        
        problemDetails.GetProperty("status").GetInt32().Should().Be(400);
        problemDetails.GetProperty("detail").GetString().Should().Contain("negative");
    }

    [Fact]
    public async Task CreateProduct_ShouldReturn400_WhenNameExceedsMaxLength()
    {
        // Arrange
        var client = _factory.CreateClient();
        var longName = new string('a', 201);
        var request = new CreateProductRequest(longName, 1299.99m, 50);

        // Act
        var response = await client.PostAsJsonAsync("/api/products", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        var content = await response.Content.ReadAsStringAsync();
        var problemDetails = JsonSerializer.Deserialize<JsonElement>(content);
        
        problemDetails.GetProperty("status").GetInt32().Should().Be(400);
        problemDetails.GetProperty("detail").GetString().Should().Contain("200 characters");
    }
}
