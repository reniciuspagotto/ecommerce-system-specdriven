using API.Requests;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace API.Tests.Integration;

public class UpdateProductIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public UpdateProductIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task UpdateProduct_EndToEnd_ShouldSucceed()
    {
        // Arrange
        var client = _factory.CreateClient();
        
        // Create a product
        var createRequest = new CreateProductRequest("Wireless Mouse", 29.99m, 100);
        var createResponse = await client.PostAsJsonAsync("/api/products", createRequest);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var createContent = await createResponse.Content.ReadAsStringAsync();
        var createdProduct = JsonSerializer.Deserialize<JsonElement>(createContent);
        var productId = createdProduct.GetProperty("id").GetGuid();

        // Act - Update the product
        var updateRequest = new 
        { 
            name = "Ergonomic Wireless Mouse", 
            price = 39.99m, 
            quantity = 75, 
            version = 1 
        };
        var updateResponse = await client.PutAsJsonAsync($"/api/products/{productId}", updateRequest);

        // Assert - Update successful
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var updateContent = await updateResponse.Content.ReadAsStringAsync();
        var updatedProduct = JsonSerializer.Deserialize<JsonElement>(updateContent);
        
        updatedProduct.GetProperty("id").GetGuid().Should().Be(productId);
        updatedProduct.GetProperty("name").GetString().Should().Be("Ergonomic Wireless Mouse");
        updatedProduct.GetProperty("price").GetDecimal().Should().Be(39.99m);
        updatedProduct.GetProperty("quantity").GetInt32().Should().Be(75);
        updatedProduct.GetProperty("version").GetInt32().Should().Be(2);
        
        // Verify retrieval returns updated data
        var getResponse = await client.GetAsync($"/api/products/{productId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var getContent = await getResponse.Content.ReadAsStringAsync();
        var retrievedProduct = JsonSerializer.Deserialize<JsonElement>(getContent);
        
        retrievedProduct.GetProperty("name").GetString().Should().Be("Ergonomic Wireless Mouse");
        retrievedProduct.GetProperty("price").GetDecimal().Should().Be(39.99m);
        retrievedProduct.GetProperty("quantity").GetInt32().Should().Be(75);
        retrievedProduct.GetProperty("version").GetInt32().Should().Be(2);
    }

    [Fact]
    public async Task UpdateProduct_WithInvalidData_ShouldReturnValidationError()
    {
        // Arrange
        var client = _factory.CreateClient();
        
        var createRequest = new CreateProductRequest("Product", 50m, 20);
        var createResponse = await client.PostAsJsonAsync("/api/products", createRequest);
        var createContent = await createResponse.Content.ReadAsStringAsync();
        var createdProduct = JsonSerializer.Deserialize<JsonElement>(createContent);
        var productId = createdProduct.GetProperty("id").GetGuid();

        // Act - Try to update with negative price
        var invalidUpdate = new { name = "Product", price = -10m, quantity = 20, version = 1 };
        var response = await client.PutAsJsonAsync($"/api/products/{productId}", invalidUpdate);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        var content = await response.Content.ReadAsStringAsync();
        var problemDetails = JsonSerializer.Deserialize<JsonElement>(content);
        
        problemDetails.GetProperty("status").GetInt32().Should().Be(400);
        problemDetails.GetProperty("detail").GetString().Should().ContainAny("price", "Price");
    }

    [Fact]
    public async Task UpdateProduct_MultipleSequentialUpdates_ShouldIncrementVersion()
    {
        // Arrange
        var client = _factory.CreateClient();
        
        var createRequest = new CreateProductRequest("Product v1", 10m, 10);
        var createResponse = await client.PostAsJsonAsync("/api/products", createRequest);
        var createContent = await createResponse.Content.ReadAsStringAsync();
        var createdProduct = JsonSerializer.Deserialize<JsonElement>(createContent);
        var productId = createdProduct.GetProperty("id").GetGuid();

        // Act & Assert - Update multiple times
        int currentVersion = 1;
        
        for (int i = 2; i <= 5; i++)
        {
            var updateRequest = new 
            { 
                name = $"Product v{i}", 
                price = 10m * i, 
                quantity = 10 * i, 
                version = currentVersion 
            };
            var response = await client.PutAsJsonAsync($"/api/products/{productId}", updateRequest);
            
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<JsonElement>(content);
            
            result.GetProperty("name").GetString().Should().Be($"Product v{i}");
            result.GetProperty("version").GetInt32().Should().Be(i);
            
            currentVersion = i;
        }
    }
}
