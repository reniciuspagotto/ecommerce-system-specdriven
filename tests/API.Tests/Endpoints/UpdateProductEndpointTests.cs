using API.Requests;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace API.Tests.Endpoints;

public class UpdateProductEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public UpdateProductEndpointTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task UpdateProduct_ShouldReturn200_WhenVersionMatches()
    {
        // Arrange
        var client = _factory.CreateClient();
        
        // Create a product
        var createRequest = new CreateProductRequest("Original Name", 100m, 50);
        var createResponse = await client.PostAsJsonAsync("/api/products", createRequest);
        var createContent = await createResponse.Content.ReadAsStringAsync();
        var createdProduct = JsonSerializer.Deserialize<JsonElement>(createContent);
        var productId = createdProduct.GetProperty("id").GetGuid();
        var version = createdProduct.GetProperty("version").GetInt32();

        // Act - Update with correct version
        var updateRequest = new { name = "Updated Name", price = 150m, quantity = 30, version };
        var updateResponse = await client.PutAsJsonAsync($"/api/products/{productId}", updateRequest);

        // Assert
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await updateResponse.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(content);
        
        result.GetProperty("id").GetGuid().Should().Be(productId);
        result.GetProperty("name").GetString().Should().Be("Updated Name");
        result.GetProperty("price").GetDecimal().Should().Be(150m);
        result.GetProperty("quantity").GetInt32().Should().Be(30);
        result.GetProperty("version").GetInt32().Should().Be(2); // Version incremented
    }

    [Fact]
    public async Task UpdateProduct_ShouldReturn409_WhenVersionMismatch()
    {
        // Arrange
        var client = _factory.CreateClient();
        
        // Create a product
        var createRequest = new CreateProductRequest("Original", 100m, 50);
        var createResponse = await client.PostAsJsonAsync("/api/products", createRequest);
        var createContent = await createResponse.Content.ReadAsStringAsync();
        var createdProduct = JsonSerializer.Deserialize<JsonElement>(createContent);
        var productId = createdProduct.GetProperty("id").GetGuid();

        // Update once to increment version
        var firstUpdate = new { name = "First Update", price = 110m, quantity = 45, version = 1 };
        await client.PutAsJsonAsync($"/api/products/{productId}", firstUpdate);

        // Act - Try to update with stale version
        var staleUpdate = new { name = "Stale Update", price = 120m, quantity = 40, version = 1 };
        var staleResponse = await client.PutAsJsonAsync($"/api/products/{productId}", staleUpdate);

        // Assert
        staleResponse.StatusCode.Should().Be(HttpStatusCode.Conflict);
        staleResponse.Content.Headers.ContentType?.MediaType.Should().Be("application/problem+json");
        
        var content = await staleResponse.Content.ReadAsStringAsync();
        var problemDetails = JsonSerializer.Deserialize<JsonElement>(content);
        
        problemDetails.GetProperty("status").GetInt32().Should().Be(409);
        problemDetails.GetProperty("title").GetString().Should().Be("Conflict");
        problemDetails.GetProperty("detail").GetString().Should().Contain("version");
    }

    [Fact]
    public async Task UpdateProduct_ShouldReturn400_WhenValidationFails()
    {
        // Arrange
        var client = _factory.CreateClient();
        
        var createRequest = new CreateProductRequest("Valid Product", 100m, 50);
        var createResponse = await client.PostAsJsonAsync("/api/products", createRequest);
        var createContent = await createResponse.Content.ReadAsStringAsync();
        var createdProduct = JsonSerializer.Deserialize<JsonElement>(createContent);
        var productId = createdProduct.GetProperty("id").GetGuid();

        // Act - Update with invalid data (empty name)
        var invalidUpdate = new { name = "", price = 100m, quantity = 50, version = 1 };
        var response = await client.PutAsJsonAsync($"/api/products/{productId}", invalidUpdate);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/problem+json");
    }

    [Fact]
    public async Task UpdateProduct_ShouldReturn404_WhenProductNotFound()
    {
        // Arrange
        var client = _factory.CreateClient();
        var nonExistentId = Guid.NewGuid();

        // Act
        var updateRequest = new { name = "Product", price = 100m, quantity = 50, version = 1 };
        var response = await client.PutAsJsonAsync($"/api/products/{nonExistentId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        
        var content = await response.Content.ReadAsStringAsync();
        var problemDetails = JsonSerializer.Deserialize<JsonElement>(content);
        
        problemDetails.GetProperty("status").GetInt32().Should().Be(404);
        problemDetails.GetProperty("detail").GetString().Should().Contain(nonExistentId.ToString());
    }

    [Fact]
    public async Task UpdateProduct_ShouldIncrementVersionSequentially()
    {
        // Arrange
        var client = _factory.CreateClient();
        
        var createRequest = new CreateProductRequest("Product", 100m, 50);
        var createResponse = await client.PostAsJsonAsync("/api/products", createRequest);
        var createContent = await createResponse.Content.ReadAsStringAsync();
        var createdProduct = JsonSerializer.Deserialize<JsonElement>(createContent);
        var productId = createdProduct.GetProperty("id").GetGuid();

        // Act & Assert - Multiple updates
        for (int expectedVersion = 1; expectedVersion <= 3; expectedVersion++)
        {
            var updateRequest = new 
            { 
                name = $"Version {expectedVersion + 1}", 
                price = 100m + (expectedVersion * 10), 
                quantity = 50, 
                version = expectedVersion 
            };
            var response = await client.PutAsJsonAsync($"/api/products/{productId}", updateRequest);
            
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<JsonElement>(content);
            result.GetProperty("version").GetInt32().Should().Be(expectedVersion + 1);
        }
    }
}
