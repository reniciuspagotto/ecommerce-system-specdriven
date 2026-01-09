using API.Requests;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace API.Tests.Endpoints;

public class DeleteProductEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public DeleteProductEndpointTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task DeleteProduct_ShouldReturn204_WhenProductExists()
    {
        // Arrange
        var client = _factory.CreateClient();
        
        // Create a product
        var createRequest = new CreateProductRequest("Product to Delete", 100m, 50);
        var createResponse = await client.PostAsJsonAsync("/api/products", createRequest);
        var createContent = await createResponse.Content.ReadAsStringAsync();
        var createdProduct = JsonSerializer.Deserialize<JsonElement>(createContent);
        var productId = createdProduct.GetProperty("id").GetGuid();

        // Act
        var deleteResponse = await client.DeleteAsync($"/api/products/{productId}");

        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        deleteResponse.Content.Headers.ContentLength.Should().Be(0);
    }

    [Fact]
    public async Task DeleteProduct_ShouldBeIdempotent_WhenProductDoesNotExist()
    {
        // Arrange
        var client = _factory.CreateClient();
        var nonExistentId = Guid.NewGuid();

        // Act
        var deleteResponse = await client.DeleteAsync($"/api/products/{nonExistentId}");

        // Assert - Idempotent: no error even if product doesn't exist
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteProduct_AfterDeletion_GetShouldReturn404()
    {
        // Arrange
        var client = _factory.CreateClient();
        
        var createRequest = new CreateProductRequest("Product", 100m, 50);
        var createResponse = await client.PostAsJsonAsync("/api/products", createRequest);
        var createContent = await createResponse.Content.ReadAsStringAsync();
        var createdProduct = JsonSerializer.Deserialize<JsonElement>(createContent);
        var productId = createdProduct.GetProperty("id").GetGuid();

        // Act - Delete product
        var deleteResponse = await client.DeleteAsync($"/api/products/{productId}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Assert - Retrieval should fail
        var getResponse = await client.GetAsync($"/api/products/{productId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteProduct_MultipleTimes_ShouldBeIdempotent()
    {
        // Arrange
        var client = _factory.CreateClient();
        
        var createRequest = new CreateProductRequest("Product", 100m, 50);
        var createResponse = await client.PostAsJsonAsync("/api/products", createRequest);
        var createContent = await createResponse.Content.ReadAsStringAsync();
        var createdProduct = JsonSerializer.Deserialize<JsonElement>(createContent);
        var productId = createdProduct.GetProperty("id").GetGuid();

        // Act - Delete multiple times
        var delete1 = await client.DeleteAsync($"/api/products/{productId}");
        var delete2 = await client.DeleteAsync($"/api/products/{productId}");
        var delete3 = await client.DeleteAsync($"/api/products/{productId}");

        // Assert - All should return 204
        delete1.StatusCode.Should().Be(HttpStatusCode.NoContent);
        delete2.StatusCode.Should().Be(HttpStatusCode.NoContent);
        delete3.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
