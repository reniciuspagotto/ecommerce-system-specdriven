using API.Requests;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace API.Tests.Integration;

public class GetProductIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public GetProductIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetProduct_EndToEnd_ShouldSucceed()
    {
        // Arrange
        var client = _factory.CreateClient();
        
        // Create a product
        var createRequest = new CreateProductRequest("Gaming Laptop", 1299.99m, 50);
        var createResponse = await client.PostAsJsonAsync("/api/products", createRequest);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var createContent = await createResponse.Content.ReadAsStringAsync();
        var createdProduct = JsonSerializer.Deserialize<JsonElement>(createContent);
        var productId = createdProduct.GetProperty("id").GetGuid();

        // Act - Retrieve the product
        var getResponse = await client.GetAsync($"/api/products/{productId}");

        // Assert
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var getContent = await getResponse.Content.ReadAsStringAsync();
        var retrievedProduct = JsonSerializer.Deserialize<JsonElement>(getContent);
        
        retrievedProduct.GetProperty("id").GetGuid().Should().Be(productId);
        retrievedProduct.GetProperty("name").GetString().Should().Be("Gaming Laptop");
        retrievedProduct.GetProperty("price").GetDecimal().Should().Be(1299.99m);
        retrievedProduct.GetProperty("quantity").GetInt32().Should().Be(50);
        retrievedProduct.GetProperty("version").GetInt32().Should().Be(1);
        
        // Timestamps should be present
        retrievedProduct.GetProperty("createdAt").GetDateTime().Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
        retrievedProduct.GetProperty("updatedAt").GetDateTime().Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task GetProduct_AfterCreate_ShouldReturnSameData()
    {
        // Arrange
        var client = _factory.CreateClient();
        var createRequest = new CreateProductRequest("Mechanical Keyboard", 149.99m, 100);
        
        // Act - Create product
        var createResponse = await client.PostAsJsonAsync("/api/products", createRequest);
        var createContent = await createResponse.Content.ReadAsStringAsync();
        var createdProduct = JsonSerializer.Deserialize<JsonElement>(createContent);
        
        // Act - Retrieve product
        var productId = createdProduct.GetProperty("id").GetGuid();
        var getResponse = await client.GetAsync($"/api/products/{productId}");
        var getContent = await getResponse.Content.ReadAsStringAsync();
        var retrievedProduct = JsonSerializer.Deserialize<JsonElement>(getContent);

        // Assert - Data should match
        retrievedProduct.GetProperty("id").GetGuid().Should().Be(createdProduct.GetProperty("id").GetGuid());
        retrievedProduct.GetProperty("name").GetString().Should().Be(createdProduct.GetProperty("name").GetString());
        retrievedProduct.GetProperty("price").GetDecimal().Should().Be(createdProduct.GetProperty("price").GetDecimal());
        retrievedProduct.GetProperty("quantity").GetInt32().Should().Be(createdProduct.GetProperty("quantity").GetInt32());
        retrievedProduct.GetProperty("version").GetInt32().Should().Be(createdProduct.GetProperty("version").GetInt32());
    }

    [Fact]
    public async Task GetProduct_WithNonExistentId_ShouldReturn404()
    {
        // Arrange
        var client = _factory.CreateClient();
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await client.GetAsync($"/api/products/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        
        var content = await response.Content.ReadAsStringAsync();
        var problemDetails = JsonSerializer.Deserialize<JsonElement>(content);
        
        problemDetails.GetProperty("status").GetInt32().Should().Be(404);
        problemDetails.GetProperty("detail").GetString().Should().Contain(nonExistentId.ToString());
    }

    [Fact]
    public async Task GetProduct_MultipleProducts_ShouldRetrieveCorrectOne()
    {
        // Arrange
        var client = _factory.CreateClient();
        
        // Create multiple products
        var product1Response = await client.PostAsJsonAsync("/api/products", 
            new CreateProductRequest("Product 1", 100m, 10));
        var product2Response = await client.PostAsJsonAsync("/api/products", 
            new CreateProductRequest("Product 2", 200m, 20));
        var product3Response = await client.PostAsJsonAsync("/api/products", 
            new CreateProductRequest("Product 3", 300m, 30));

        var product1 = JsonSerializer.Deserialize<JsonElement>(await product1Response.Content.ReadAsStringAsync());
        var product2 = JsonSerializer.Deserialize<JsonElement>(await product2Response.Content.ReadAsStringAsync());
        var product3 = JsonSerializer.Deserialize<JsonElement>(await product3Response.Content.ReadAsStringAsync());

        var id1 = product1.GetProperty("id").GetGuid();
        var id2 = product2.GetProperty("id").GetGuid();
        var id3 = product3.GetProperty("id").GetGuid();

        // Act - Retrieve product 2
        var response = await client.GetAsync($"/api/products/{id2}");
        var content = await response.Content.ReadAsStringAsync();
        var retrieved = JsonSerializer.Deserialize<JsonElement>(content);

        // Assert - Should get product 2, not 1 or 3
        retrieved.GetProperty("id").GetGuid().Should().Be(id2);
        retrieved.GetProperty("name").GetString().Should().Be("Product 2");
        retrieved.GetProperty("price").GetDecimal().Should().Be(200m);
        retrieved.GetProperty("quantity").GetInt32().Should().Be(20);
    }
}
