using API.Requests;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace API.Tests.Integration;

public class CreateProductIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public CreateProductIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CreateProduct_EndToEnd_ShouldSucceed()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new CreateProductRequest("Gaming Laptop", 1299.99m, 50);

        // Act - Create product
        var createResponse = await client.PostAsJsonAsync("/api/products", request);

        // Assert - Product created
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var createContent = await createResponse.Content.ReadAsStringAsync();
        var createdProduct = JsonSerializer.Deserialize<JsonElement>(createContent);
        var productId = createdProduct.GetProperty("id").GetGuid();

        productId.Should().NotBeEmpty();
        createdProduct.GetProperty("name").GetString().Should().Be("Gaming Laptop");
        createdProduct.GetProperty("price").GetDecimal().Should().Be(1299.99m);
        createdProduct.GetProperty("quantity").GetInt32().Should().Be(50);
        createdProduct.GetProperty("version").GetInt32().Should().Be(1);
        
        // Location header should point to the new resource
        createResponse.Headers.Location.Should().NotBeNull();
        createResponse.Headers.Location!.ToString().Should().Contain($"/api/products/{productId}");
    }

    [Fact]
    public async Task CreateProduct_WithInvalidData_ShouldReturnValidationErrors()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new CreateProductRequest("", 0, -1);

        // Act
        var response = await client.PostAsJsonAsync("/api/products", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/problem+json");
        
        var content = await response.Content.ReadAsStringAsync();
        var problemDetails = JsonSerializer.Deserialize<JsonElement>(content);
        
        problemDetails.GetProperty("type").GetString().Should().Contain("rfc7231");
        problemDetails.GetProperty("title").GetString().Should().Be("Bad Request");
        problemDetails.GetProperty("status").GetInt32().Should().Be(400);
        problemDetails.GetProperty("detail").GetString().Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task CreateProduct_MultipleProducts_ShouldHaveUniqueIds()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request1 = new CreateProductRequest("Product 1", 100m, 10);
        var request2 = new CreateProductRequest("Product 2", 200m, 20);

        // Act
        var response1 = await client.PostAsJsonAsync("/api/products", request1);
        var response2 = await client.PostAsJsonAsync("/api/products", request2);

        // Assert
        response1.StatusCode.Should().Be(HttpStatusCode.Created);
        response2.StatusCode.Should().Be(HttpStatusCode.Created);

        var content1 = await response1.Content.ReadAsStringAsync();
        var content2 = await response2.Content.ReadAsStringAsync();
        
        var product1 = JsonSerializer.Deserialize<JsonElement>(content1);
        var product2 = JsonSerializer.Deserialize<JsonElement>(content2);
        
        var id1 = product1.GetProperty("id").GetGuid();
        var id2 = product2.GetProperty("id").GetGuid();
        
        id1.Should().NotBe(id2);
    }
}
