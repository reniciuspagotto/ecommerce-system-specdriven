using API.Requests;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace API.Tests.Endpoints;

public class GetProductEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public GetProductEndpointTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetProduct_ShouldReturn200_WhenProductExists()
    {
        // Arrange
        var client = _factory.CreateClient();
        
        // First create a product
        var createRequest = new CreateProductRequest("Gaming Laptop", 1299.99m, 50);
        var createResponse = await client.PostAsJsonAsync("/api/products", createRequest);
        var createContent = await createResponse.Content.ReadAsStringAsync();
        var createdProduct = JsonSerializer.Deserialize<JsonElement>(createContent);
        var productId = createdProduct.GetProperty("id").GetGuid();

        // Act
        var response = await client.GetAsync($"/api/products/{productId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(content);
        
        result.GetProperty("id").GetGuid().Should().Be(productId);
        result.GetProperty("name").GetString().Should().Be("Gaming Laptop");
        result.GetProperty("price").GetDecimal().Should().Be(1299.99m);
        result.GetProperty("quantity").GetInt32().Should().Be(50);
        result.GetProperty("version").GetInt32().Should().Be(1);
    }

    [Fact]
    public async Task GetProduct_ShouldReturn404_WhenProductDoesNotExist()
    {
        // Arrange
        var client = _factory.CreateClient();
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await client.GetAsync($"/api/products/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/problem+json");
        
        var content = await response.Content.ReadAsStringAsync();
        var problemDetails = JsonSerializer.Deserialize<JsonElement>(content);
        
        problemDetails.GetProperty("type").GetString().Should().Contain("rfc7231");
        problemDetails.GetProperty("title").GetString().Should().Be("Not Found");
        problemDetails.GetProperty("status").GetInt32().Should().Be(404);
        problemDetails.GetProperty("detail").GetString().Should().Contain(nonExistentId.ToString());
    }

    [Fact]
    public async Task GetProduct_ShouldReturnCorrectContentType()
    {
        // Arrange
        var client = _factory.CreateClient();
        
        var createRequest = new CreateProductRequest("Gaming Laptop", 1299.99m, 50);
        var createResponse = await client.PostAsJsonAsync("/api/products", createRequest);
        var createContent = await createResponse.Content.ReadAsStringAsync();
        var createdProduct = JsonSerializer.Deserialize<JsonElement>(createContent);
        var productId = createdProduct.GetProperty("id").GetGuid();

        // Act
        var response = await client.GetAsync($"/api/products/{productId}");

        // Assert
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");
    }
}
